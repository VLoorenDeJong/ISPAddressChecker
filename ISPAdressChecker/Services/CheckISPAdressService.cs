using ISPAddressChecker.Helpers;
using ISPAddressChecker.Interfaces;
using ISPAddressChecker.Options;
using ISPAddressChecker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.RegularExpressions;

public class CheckISPAddressService : ICheckISPAddressService
{
    private readonly ApplicationSettingsOptions _applicationSettingsOptions;
    private readonly IISPAddressCounterService _counterService;
    private readonly IISPAddressService _ISPAddressService;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    private Dictionary<string, string> ISPAddressChecks = new();

    public CheckISPAddressService(ILogger<CheckISPAddressService> logger, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, IEmailService emailService, IISPAddressCounterService counterService, IISPAddressService ISPAddressService)
    {
        _logger = logger;
        _applicationSettingsOptions = applicationSettingsOptions?.Value!;
        _emailService = emailService;
        _counterService = counterService;
        _ISPAddressService = ISPAddressService;
    }

    public async Task HeartBeatCheck()
    {
        _logger.LogInformation("HeartBeatCheck -> start");
        await GetISPAddressFromBackupAPIs(true);
        _emailService.SendHeartBeatEmail(_counterService, _ISPAddressService.GetOldISPAddress(), _ISPAddressService.GetCurrentISPAddress(), _ISPAddressService.GetNewISPAddress(), ISPAddressChecks);
        ISPAddressChecks.Clear();
    }

    public async Task GetISPAddressAsync()
    {
        using (var client = new HttpClient())
        {
            try
            {
                _logger.LogInformation("GetISPAddressAsync: Requesting ISP address from endpoint");
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


                _logger.LogInformation("GetISPAddressAsync -> NewISPAddress before clear:{ispAddress}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));

                _ISPAddressService.ClearNewISPAddress();


                string fecthedISPAddress = await response?.Content?.ReadAsStringAsync()!;
                _ISPAddressService.SetNewISPAddress(fecthedISPAddress);

                _logger.LogInformation("GetISPAddressAsync: Respons:{ispAddress}", StringHelpers.MakeISPAddressLogReady(fecthedISPAddress));
                _logger.LogInformation("GetISPAddressAsync -> New NewISPAddress:{ispAddress}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));


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

        _logger.LogInformation("GetISPAddressAsync -> if(NewISPAddress && CurrentISPAddress same) Connection reestablished -> {isp1}->{isp2}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()));
        if (!string.Equals(_ISPAddressService.GetNewISPAddress(), _ISPAddressService.GetCurrentISPAddress(), StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogInformation("GetISPAddressAsync -> Connection reestablished");
            // Copy the old ISP address to that variable

            _logger.LogInformation("GetISPAddressAsync -> Old BEFORE change:{oldISP}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetOldISPAddress()));
            _ISPAddressService.SetOldISPAddress(_ISPAddressService.GetCurrentISPAddress());
            _logger.LogInformation("GetISPAddressAsync -> Old AFTER change:{oldISP}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetOldISPAddress()));

            // Make the new ISP address the current address
            _logger.LogInformation("GetISPAddressAsync -> GetNewISPAddress BEFORE change:{newISP}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));

            _ISPAddressService.SetCurrentISPAddress(_ISPAddressService.GetNewISPAddress());

            _logger.LogInformation("GetISPAddressAsync -> GetNewISPAddress AFTER change:{newISP}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));

            _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail, NewISP: {newISP}, Old ISP: {oldISP}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetOldISPAddress()));
            _emailService.SendConnectionReestablishedEmail(_ISPAddressService.GetNewISPAddress(), _ISPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);

            _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail -> Before reset FailedCOunter{counter1}, ExternalISPAddress: {exIISP}, NewISP: {newISp}", _counterService.GetFailedISPRequestCounter(), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetExternalISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));

            _counterService.ResetFailedISPRequestCounter();
            _ISPAddressService.ClearExternalISPAddress();
            _ISPAddressService.ClearNewISPAddress();

            _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail -> After reset FailedCOunter{counter1}, ExternalISPAddress: {exIISP}, NewISP: {newISp}", _counterService.GetFailedISPRequestCounter(), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetExternalISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));
        }
        else
        {
            _logger.LogInformation("GetISPAddressAsync -> ISP address not changed -> ISPAddress:{isp}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()));
        }
    }

    public async Task GetISPAddressFromBackupAPIs(bool heartBeatCheck)
    {
        _logger.LogInformation("GetISPAddressFromBackupAPIs -> External call started, external call counter:{count}", _counterService.GetExternalServiceCheckCounter());

        if (ISPAddressChecks is null) ISPAddressChecks = new();
        ISPAddressChecks.Clear();

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
                        ISPAddress = match.Value; // Output: ISP address
                    }

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> URL:{APIUrl} Respons:{ispAddress}", APIUrl, StringHelpers.MakeISPAddressLogReady(ISPAddress));
                    ISPAddressChecks.Add(APIUrl!, ISPAddress);

                    // Testing code            
                    //ISPAddressChecks.Add("112323", "1236");
                    //ISPAddressChecks.Add("1dfa23", "132136");
                    //ISPAddressChecks.Add("213123", "12124asc36");
                    //ISPAddressChecks.Add("12zcx q343", "12123asd36");
                    //ISPAddressChecks.Add("1234321yg1q ", "11243rwqr236");
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

        _logger.LogInformation("GetISPAddressFromBackupAPIs -> ExternalResponseCount:{count}", ISPAddressChecks.Count);
        if (ISPAddressChecks.Count > 0)
        {
            _logger.LogInformation("GetISPAddressFromBackupAPIs -> More then one result:{count}", ISPAddressChecks.Count);
            // Get the uniwue ISP addresses from the dictionary
            List<string>? uniqueAddresses = ISPAddressChecks?.Values?.Distinct()?.ToList()!;


            if (uniqueAddresses!.Count == 1)
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> {count}x Same ISPAddress response:{ISPA}", ISPAddressChecks!.Count, StringHelpers.MakeISPAddressLogReady(uniqueAddresses[0]!));
                // Update new ISP address

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> External ISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetExternalISPAddress()));

                _ISPAddressService.SetExternalISPAddress(uniqueAddresses[0]!);

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> External ISPAddress AFTER set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetExternalISPAddress()));

                // Copy the old ISP address to that variable
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()));
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> OldISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetOldISPAddress()));

                _ISPAddressService.SetOldISPAddress(_ISPAddressService.GetCurrentISPAddress());

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> OldISPAddress AFTER set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetOldISPAddress()));


                _logger.LogInformation("GetISPAddressFromBackupAPIs -> HeartBeatCheck:{heartbeat}", heartBeatCheck);
                if (!heartBeatCheck)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> GetServiceRequestCounter: {count1}, GetFailedISPRequestCounter: {count2} if(1 & 0 SendISPAddressChangedEmail)", _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                    if (_counterService.GetServiceRequestCounter() != 1 && _counterService.GetFailedISPRequestCounter() != 0)
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE ClearCurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()));
                        _ISPAddressService.ClearCurrentISPAddress();
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER ClearCurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()));

                        _emailService.SendISPAddressChangedEmail(_ISPAddressService.GetExternalISPAddress(), _ISPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> GetServiceRequestCounter not if(1): {count1}, GetFailedISPRequestCounter not if(0):{count2}", _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                    }
                    else
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> First setup");

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()));
                        _ISPAddressService.SetCurrentISPAddress(_ISPAddressService.GetExternalISPAddress());
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER set:{ISP1} expected: {ISP2}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetCurrentISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetExternalISPAddress()));

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> NewISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()));
                        _ISPAddressService.SetNewISPAddress(_ISPAddressService.GetExternalISPAddress());
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> NewISPAddress AFTER set:{ISP1} expected: {ISP2}", StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_ISPAddressService.GetExternalISPAddress()));

                    }
                }
            }
            else
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> Different ISPAddresses returned");
                foreach (KeyValuePair<string, string> item in ISPAddressChecks!)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> Different ISPAddresses returned -> URL: {Url} -> {ISP}", item.Key, StringHelpers.MakeISPAddressLogReady(item.Value));
                }
                _emailService.SendDifferendISPAddressValuesEmail(ISPAddressChecks!, _ISPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);
            }
        }
        else if (ISPAddressChecks.Count == 0)
        {
            _logger.LogInformation("GetISPAddressFromBackupAPIs -> No external results");
            _emailService.SendNoISPAddressReturnedEmail(_ISPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.TimeIntervalInMinutes);
        }
    }
}
