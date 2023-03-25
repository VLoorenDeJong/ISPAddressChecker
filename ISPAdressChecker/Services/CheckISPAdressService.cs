using ISPAdressChecker.Services.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ISPAdressChecker.Helpers;
using ISPAdressChecker.Options;

public class CheckISPAddressService : ICheckISPAddressService
{
    private readonly ApplicationSettingsOptions _applicationSettingsOptions;
    private readonly IISPAdressCounterService _counterService;
    private readonly IISPAddressService _ISPAdressService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    private Dictionary<string, string> ISPAdressChecks = new();

    public CheckISPAddressService(ILogger<CheckISPAddressService> logger, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, IEmailService emailService, IISPAdressCounterService counterService, IISPAddressService ISPAdressService)
    {
        _logger = logger;
        _applicationSettingsOptions = applicationSettingsOptions?.Value!;
        _emailService = emailService;
        _counterService = counterService;
        _ISPAdressService = ISPAdressService;
    }

    public async Task HeartBeatCheck()
    {
        _logger.LogInformation("HeartBeatCheck -> start");
        await GetISPAddressFromBackupAPIs(true);
        _emailService.SendHeartBeatEmail(_counterService, _ISPAdressService.GetOldISPAddress(), _ISPAdressService.GetCurrentISPAddress(), _ISPAdressService.GetNewISPAddress(), ISPAdressChecks);
        ISPAdressChecks.Clear();
    }

    public async Task GetISPAddressAsync()
    {
        using (var client = new HttpClient())
        {
            try
            {
                _logger.LogInformation("GetISPAddressAsync: Requesting ISP adress from endpoint");
                //Testing code:
                //throw new HttpRequestException();
                //throw new Exception();
                //_counterService.AddExternalServiceCheckCounter();
                //_counterService.AddServiceRequestCounter();

                _counterService!.AddServiceRequestCounter();


                // Testing code
                //if (_counterService!.GetServiceCheckCounter() == 5) 
                //{
                //    _logger.LogInformation("GetISPAddressAsync ->  GetService counter:{count} == 5 => mocing endpoint not found", _counterService!.GetServiceCheckCounter());
                //    throw new HttpRequestException("Service Unavailable", null, HttpStatusCode.ServiceUnavailable); 
                //};

                HttpResponseMessage response = await client.GetAsync(_applicationSettingsOptions?.APIEndpointURL);
                response.EnsureSuccessStatusCode();


                _logger.LogInformation("GetISPAddressAsync -> NewISPAddress before clear:{ispAdress}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));

                _ISPAdressService.ClearNewISPAddress();


                string fecthedISPAddress = await response?.Content?.ReadAsStringAsync()!;
                _ISPAdressService.SetNewISPAddress(fecthedISPAddress);

                _logger.LogInformation("GetISPAddressAsync: Respons:{ispAdress}", StringHelpers.MakeISPAddressLogReady(fecthedISPAddress));
                _logger.LogInformation("GetISPAddressAsync -> New NewISPAddress:{ispAdress}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));


                // Checking if the counters are still in sync 
                if (_counterService.GetServiceRequestCounter() != _counterService.GetServiceCheckCounter())
                {
                    _emailService.SendCounterDifferenceEmail(_counterService);
                    _logger.LogInformation("GetISPAddressAsync -> Counter difference ServiceRequestCounter:{counter1}, ServiceCheckCounter: {counter2}", _counterService.GetServiceRequestCounter(), _counterService.GetServiceCheckCounter());
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _counterService.AddFailedISPRequestCounter();
                    _logger.LogInformation("GetISPAddressAsync -> HttpStatusCode.ServiceUnavailable, starting external calls");
                    await GetISPAddressFromBackupAPIs(false);
                }
                else
                {
                    Type exceptionType = ex.GetType();

                    _logger.LogError("GetISPAddressAsync -> API Call HTTP exception. Exceptiontype: {type} Message:{message}", exceptionType, ex.Message);
                    _emailService.SendISPAPIHTTPExceptionEmail(exceptionType.Name, ex.Message);

                    _logger.LogInformation("GetISPAddressAsync -> API endpoint not found, starting external calls");
                    await GetISPAddressFromBackupAPIs(false);
                }
                return;
            }
            catch (Exception ex)
            {
                Type exceptionType = ex.GetType();

                _logger.LogError("GetISPAddressAsync -> API Call general Exception. Exceptiontype: {type} Message:{message}", exceptionType, ex.Message);
                _emailService.SendISPAPIExceptionEmail(exceptionType.Name, ex.Message);
                return;
            }
        }

        _logger.LogInformation("GetISPAddressAsync -> if(NewISPAddress && CurrentISPAddress same) Connection reestablished -> {isp1}->{isp2}", _ISPAdressService.GetNewISPAddress(), _ISPAdressService.GetCurrentISPAddress());
        if (!string.Equals(_ISPAdressService.GetNewISPAddress(), _ISPAdressService.GetCurrentISPAddress(), StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogInformation("GetISPAddressAsync -> Connection reestablished");
            // Copy the old ISP adress to that variable

            _logger.LogInformation("GetISPAddressAsync -> Old BEFORE change:{oldISP}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetOldISPAddress()));
            _ISPAdressService.SetOldISPAddress(_ISPAdressService.GetCurrentISPAddress());
            _logger.LogInformation("GetISPAddressAsync -> Old AFTER change:{oldISP}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetOldISPAddress()));

            // Make the new ISP address the current address
            _logger.LogInformation("GetISPAddressAsync -> GetNewISPAddress BEFORE change:{newISP}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));

            _ISPAdressService.SetCurrentISPAddress(_ISPAdressService.GetNewISPAddress());

            _logger.LogInformation("GetISPAddressAsync -> GetNewISPAddress AFTER change:{newISP}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));

            _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail, NewISP: {newISP}, Old ISP: {oldISP}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetOldISPAddress()));
            _emailService.SendConnectionReestablishedEmail(_ISPAdressService.GetNewISPAddress(), _ISPAdressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);

            _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail -> Before reset FailedCOunter{counter1}, ExternalISPAddress: {exIISP}, NewISP: {newISp}", _counterService.GetFailedISPRequestCounter(), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetExternalISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));

            _counterService.ResetFailedISPRequestCounter();
            _ISPAdressService.ClearExternalISPAddress();
            _ISPAdressService.ClearNewISPAddress();

            _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail -> After reset FailedCOunter{counter1}, ExternalISPAddress: {exIISP}, NewISP: {newISp}", _counterService.GetFailedISPRequestCounter(), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetExternalISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));
        }
        else
        {
            _logger.LogInformation("GetISPAddressAsync -> ISP adress not changed -> ISPAddress:{isp}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetCurrentISPAddress()));
        }
    }

    public async Task GetISPAddressFromBackupAPIs(bool heartBeatCheck)
    {
        _logger.LogInformation("GetISPAddressFromBackupAPIs -> External call started, external call counter:{count}", _counterService.GetExternalServiceCheckCounter());

        if (ISPAdressChecks is null) ISPAdressChecks = new();
        ISPAdressChecks.Clear();

        _counterService.AddExternalServiceCheckCounter();

        foreach (string? APIUrl in _applicationSettingsOptions?.BackupAPIS!)
        {
            _logger.LogInformation("GetISPAddressFromBackupAPIs -> Fecthing URL:{APIUrl}", APIUrl);
            // Testing code
            //int APICallCounter = 1;
            using (var client = new HttpClient())
            {
                try
                {
                    // Testing code
                    //APICallCounter++;
                    //if (APICallCounter == 2) throw new HttpRequestException();
                    //APICallCounter++;
                    //throw new HttpRequestException();
                    //throw new Exception();

                    HttpResponseMessage response = await client.GetAsync(APIUrl);
                    response.EnsureSuccessStatusCode();

                    string ISPAddress = await response.Content.ReadAsStringAsync();

                    Match match = Regex.Match(ISPAddress, @"\b(?:\d{1,3}\.){3}\d{1,3}\b");
                    if (match.Success)
                    {
                        ISPAddress = match.Value; // Output: ISP adress
                    }

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> URL:{APIUrl} Respons:{ispAdress}", APIUrl, StringHelpers.MakeISPAddressLogReady(ISPAddress));
                    ISPAdressChecks.Add(APIUrl!, ISPAddress);

                    // Testing code            
                    //ISPAdressChecks.Add("112323", "1236");
                    //ISPAdressChecks.Add("1dfa23", "132136");
                    //ISPAdressChecks.Add("213123", "12124asc36");
                    //ISPAdressChecks.Add("12zcx q343", "12123asd36");
                    //ISPAdressChecks.Add("1234321yg1q ", "11243rwqr236");
                }
                catch (HttpRequestException ex)
                {
                    Type exceptionType = ex.GetType();

                    _logger.LogError("GetISPAddressFromBackupAPIs -> API Call HttpRequestException -> URL:{APIUrl}. Exceptiontype: {type} Message:{message}", APIUrl, exceptionType, ex.Message);

                    _emailService.SendExternalAPIHTTPExceptionEmail(APIUrl!, exceptionType.Name, ex.Message);

                }
                catch (Exception ex)
                {

                    Type exceptionType = ex.GetType();

                    _logger.LogError("GetISPAddressFromBackupAPIs -> API Call Exception -> URL:{APIUrl}. Exceptiontype: {type} Message:{message}", APIUrl, exceptionType, ex.Message);

                    _emailService.SendExternalAPIExceptionEmail(APIUrl!, exceptionType.Name, ex.Message);
                }
            }
        }

        _logger.LogInformation("GetISPAddressFromBackupAPIs -> ExternalResponseCount:{count}", ISPAdressChecks.Count);
        if (ISPAdressChecks.Count > 0)
        {
            _logger.LogInformation("GetISPAddressFromBackupAPIs -> More then one result:{count}", ISPAdressChecks.Count);
            // Get the uniwue ISP adresses from the dictionary
            List<string>? uniqueAdresses = ISPAdressChecks?.Values?.Distinct()?.ToList()!;


            if (uniqueAdresses!.Count == 1)
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> {count}x Same ISPAddress response:{ISPA}", ISPAdressChecks!.Count, StringHelpers.MakeISPAddressLogReady(uniqueAdresses[0]!));
                // Update new ISP adress

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> External ISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetExternalISPAddress()));

                _ISPAdressService.SetExternalISPAddress(uniqueAdresses[0]!);

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> External ISPAddress AFTER set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetExternalISPAddress()));

                // Copy the old ISP adress to that variable
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetCurrentISPAddress()));
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> OldISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetOldISPAddress()));

                _ISPAdressService.SetOldISPAddress(_ISPAdressService.GetCurrentISPAddress());

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> OldISPAddress AFTER set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetOldISPAddress()));




                _logger.LogInformation("GetISPAddressFromBackupAPIs -> HeartBeatCheck:{heartbeat} GetServiceRequestCounter: {count1}, GetFailedISPRequestCounter: {count2} if(1 & 0 SendISPAdressChangedEmail)", heartBeatCheck, _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                if (!heartBeatCheck && _counterService.GetServiceRequestCounter() != 1 && _counterService.GetFailedISPRequestCounter() != 0)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE ClearCurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetCurrentISPAddress()));
                    _ISPAdressService.ClearCurrentISPAddress();
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER ClearCurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetCurrentISPAddress()));

                    _emailService.SendISPAdressChangedEmail(_ISPAdressService.GetExternalISPAddress(), _ISPAdressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> GetServiceRequestCounter not if(1): {count1}, GetFailedISPRequestCounter not if(0):{count2}", _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                }
                else if (!heartBeatCheck)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> First setup");

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetCurrentISPAddress()));
                    _ISPAdressService.SetCurrentISPAddress(_ISPAdressService.GetExternalISPAddress());
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER set:{ISP1} expected: {ISP2}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetCurrentISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetExternalISPAddress()));

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> NewISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()));
                    _ISPAdressService.SetNewISPAddress(_ISPAdressService.GetExternalISPAddress());
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> NewISPAddress AFTER set:{ISP1} expected: {ISP2}", StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAdressService.GetExternalISPAddress()));

                }
            }
            else
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> Different ISPAdresses returned");
                foreach (KeyValuePair<string, string> item in ISPAdressChecks!)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> Different ISPAdresses returned -> URL: {Url} -> {ISP}", item.Key, StringHelpers.MakeISPAddressLogReady(item.Value));
                }
                _emailService.SendDifferendISPAdressValuesEmail(ISPAdressChecks!, _ISPAdressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);
            }
        }
        else if (ISPAdressChecks.Count == 0)
        {
            _logger.LogInformation("GetISPAddressFromBackupAPIs -> No external results");
            _emailService.SendNoISPAdressReturnedEmail(_ISPAdressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);
        }
    }
}
