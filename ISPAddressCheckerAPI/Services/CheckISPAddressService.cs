using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Helpers;
using ISPAddressChecker.Options;
using ISPAddressChecker.Interfaces;

namespace ISPAddressCheckerAPI.Services
{
    public class CheckISPAddressService : ICheckISPAddressService
    {
        private readonly string serviceName = nameof(CheckISPAddressService);


        private readonly APIApplicationSettingsOptions _applicationSettingsOptions;
        private readonly APIEmailSettingsOptions? _emailSettingsOptions;

        private readonly IISPAddressCounterService _counterService;
        private readonly IISPAddressService _iSPAddressService;
        private readonly IAPIEmailService _emailService;
        private readonly ILogger<CheckISPAddressService> _logger;
        private readonly ILogHubService _logHub;
        private Dictionary<string, string> ISPAddressChecks = new();

        public CheckISPAddressService(ILogger<CheckISPAddressService> logger
                                     , IOptions<APIApplicationSettingsOptions> applicationSettingsOptions
                                     , IOptions<APIEmailSettingsOptions> emailSettingsOptions
                                     , IAPIEmailService emailService, IISPAddressCounterService counterService
                                     , IISPAddressService ISPAddressService, ILogHubService logHub
                                      )
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;
            _emailService = emailService;
            _counterService = counterService;
            _iSPAddressService = ISPAddressService;
            _logHub = logHub;
            _emailSettingsOptions = emailSettingsOptions?.Value;
        }

        public async Task HeartBeatCheck(TimeSpan uptime)
        {
            _logger.LogInformation("HeartBeatCheck -> start");
            await GetISPAddressFromBackupAPIs(true);
            if (_emailSettingsOptions!.HeartbeatEmailEnabled)
            {
                await _emailService.SendHeartBeatEmail(_counterService
                                                      , _iSPAddressService.GetOldISPAddress()
                                                      , _iSPAddressService.GetCurrentISPAddress()
                                                      , _iSPAddressService.GetNewISPAddress()
                                                      , ISPAddressChecks, _emailService.APIEmailDetails
                                                      , uptime
                                                       );
            }
            ISPAddressChecks.Clear();
        }

        public async Task GetISPAddressAsync()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    _logger.LogInformation("GetISPAddressAsync -> Requesting ISP adress from endpoint");
                    await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressAsync -> Requesting ISP adress from endpoint");


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

                    string fecthedISPAddress = await response?.Content?.ReadAsStringAsync()!;

                    if (!string.IsNullOrWhiteSpace(fecthedISPAddress))
                    {
                        _counterService.AddSuccessFullRequestsCounter();

                        _logger.LogInformation("GetISPAddressAsync -> NewISPAddress before clear:{ispAddress}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> NewISPAddress before clear: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");

                        _iSPAddressService.ClearNewISPAddress();
                        _iSPAddressService.SetNewISPAddress(fecthedISPAddress);
                    }


                    _logger.LogInformation("GetISPAddressAsync -> Respons:{ispAddress}", StringHelpers.MakeISPAddressLogReady(fecthedISPAddress));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> Respons: {StringHelpers.MakeISPAddressLogReady(fecthedISPAddress)}");

                    _logger.LogInformation("GetISPAddressAsync -> New NewISPAddress:{ispAddress}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> New NewISPAddress: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");


                    // Checking if the counters are still in sync 
                    if (_counterService.GetServiceRequestCounter() != _counterService.GetServiceCheckCounter())
                    {
                        await _emailService.SendCounterDifferenceEmail(_counterService);
                        _logger.LogInformation("GetISPAddressAsync -> Counter difference ServiceRequestCounter:{counter1}, ServiceCheckCounter: {counter2}", _counterService.GetServiceRequestCounter(), _counterService.GetServiceCheckCounter());
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> Counter difference ServiceRequestCounter: {_counterService.GetServiceRequestCounter()}, ServiceCheckCounter: {_counterService.GetServiceCheckCounter()}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        _counterService.AddFailedISPRequestCounter();

                        _logger.LogInformation("GetISPAddressAsync -> HttpStatusCode.ServiceUnavailable, starting external calls");
                        await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressAsync -> HttpStatusCode.ServiceUnavailable, starting external calls");

                        await GetISPAddressFromBackupAPIs(false);
                    }
                    else
                    {
                        Type exceptionType = ex.GetType();

                        _logger.LogError("GetISPAddressAsync -> API Call HTTP exception. Exceptiontype: {type} Message:{message}", exceptionType, ex.Message);
                        await _logHub.SendLogErrorAsync(serviceName, $"GetISPAddressAsync -> API Call HTTP exception. Exceptiontype: {exceptionType}, Message:{ex.Message}");

                        await _emailService.SendISPAPIHTTPExceptionEmail(exceptionType.Name, ex.Message);

                        _logger.LogInformation("GetISPAddressAsync -> API endpoint not found, starting external calls");
                        await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressAsync -> API endpoint not found, starting external calls");

                        await GetISPAddressFromBackupAPIs(false);
                    }
                    return;
                }
                catch (Exception ex)
                {
                    Type exceptionType = ex.GetType();

                    _counterService.AddFailedISPRequestCounter();

                    _logger.LogError("GetISPAddressAsync -> API Call general Exception. Exceptiontype: {type} Message:{message}", exceptionType, ex.Message);
                    await _logHub.SendLogErrorAsync(serviceName, $"GetISPAddressAsync -> API Call general Exception. Exceptiontype: {exceptionType}, Message:{ex.Message}");

                    await _emailService.SendISPAPIExceptionEmail(exceptionType.Name, ex.Message);
                    return;
                }
            }

            _logger.LogInformation("GetISPAddressAsync -> if(NewISPAddress && CurrentISPAddress same) Connection reestablished -> {isp1}->{isp2}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()));
            await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> if(NewISPAddress && CurrentISPAddress same) Connection reestablished -> {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}->{StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())}");

            if (!string.Equals(_iSPAddressService.GetNewISPAddress(), _iSPAddressService.GetCurrentISPAddress(), StringComparison.CurrentCultureIgnoreCase))
            {
                _logger.LogInformation("GetISPAddressAsync -> Connection reestablished");
                await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressAsync -> Connection reestablished");

                // Copy the old ISP adress to that variable

                _logger.LogInformation("GetISPAddressAsync -> Old BEFORE change:{oldISP}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> Old BEFORE change:{StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress())}");

                _iSPAddressService.SetOldISPAddress(_iSPAddressService.GetCurrentISPAddress());
                _logger.LogInformation("GetISPAddressAsync -> Old AFTER change:{oldISP}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> Old AFTER change:{StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress())}");

                // Make the new ISP address the current address
                _logger.LogInformation("GetISPAddressAsync -> GetNewISPAddress BEFORE change:{newISP}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> GetNewISPAddress BEFORE change:{StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");

                _iSPAddressService.SetCurrentISPAddress(_iSPAddressService.GetNewISPAddress());

                _logger.LogInformation("GetISPAddressAsync -> GetNewISPAddress AFTER change:{newISP}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> GetNewISPAddress AFTER change:{StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");

                _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail, NewISP: {newISP}, Old ISP: {oldISP}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> SendConnectionReestablishedEmail, NewISP: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}, Old ISP: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress())}");

                await _emailService.SendConnectionReestablishedEmail(_iSPAddressService.GetNewISPAddress(), _iSPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes);

                _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail -> Before reset FailedCOunter{counter1}, ExternalISPAddress: {exIISP}, NewISP: {newISp}", _counterService.GetFailedISPRequestCounter(), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress()), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> SendConnectionReestablishedEmail -> Before reset FailedCOunter{_counterService.GetFailedISPRequestCounter()}, ExternalISPAddress: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress())}, NewISP: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");

                _counterService.ResetFailedISPRequestCounter();
                _iSPAddressService.ClearExternalISPAddress();
                _iSPAddressService.ClearNewISPAddress();

                _logger.LogInformation("GetISPAddressAsync -> SendConnectionReestablishedEmail -> After reset FailedCOunter{counter1}, ExternalISPAddress: {exIISP}, NewISP: {newISp}", _counterService.GetFailedISPRequestCounter(), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress()), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> SendConnectionReestablishedEmail -> After reset FailedCOunter{_counterService.GetFailedISPRequestCounter()}, ExternalISPAddress: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress())}, NewISP: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");
            }
            else
            {
                _logger.LogInformation("GetISPAddressAsync -> ISP adress not changed -> ISPAddress:{isp}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()));
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressAsync -> ISP adress not changed -> ISPAddress:{StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())}");
            }
        }

        public async Task GetISPAddressFromBackupAPIs(bool heartBeatCheck)
        {
            _logger.LogInformation("GetISPAddressFromBackupAPIs -> External call started, external call counter:{count}", _counterService.GetExternalServiceUsekCounter());
            await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> External call started, external call counter: {_counterService.GetExternalServiceUsekCounter()}");

            if (ISPAddressChecks is null) ISPAddressChecks = new();
            ISPAddressChecks.Clear();

            _counterService.AddExternalServiceUseCounter();

            foreach (string? APIUrl in _applicationSettingsOptions?.BackupAPIS!)
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> Fecthing URL:{APIUrl}", APIUrl);
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> Fecthing URL:{APIUrl}");

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

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> URL:{APIUrl} Respons:{ispAddress}", APIUrl, StringHelpers.MakeISPAddressLogReady(ISPAddress));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> URL:{APIUrl} Respons:{StringHelpers.MakeISPAddressLogReady(ISPAddress)}");

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
                        await _logHub.SendLogErrorAsync(serviceName, $"GetISPAddressFromBackupAPIs -> API Call HttpRequestException -> URL:{APIUrl}. Exceptiontype: {exceptionType}, Message:{ex.Message}");

                        await _emailService.SendExternalAPIHTTPExceptionEmail(APIUrl!, exceptionType.Name, ex.Message);

                    }
                    catch (Exception ex)
                    {

                        Type exceptionType = ex.GetType();

                        _logger.LogError("GetISPAddressFromBackupAPIs -> API Call Exception -> URL:{APIUrl}. Exceptiontype: {type} Message:{message}", APIUrl, exceptionType, ex.Message);
                        await _logHub.SendLogErrorAsync(serviceName, $"GetISPAddressFromBackupAPIs -> API Call Exception -> URL:{APIUrl}. Exceptiontype: {exceptionType}, Message:{ex.Message}");

                        await _emailService.SendExternalAPIExceptionEmail(APIUrl!, exceptionType.Name, ex.Message);
                    }
                }
            }

            _logger.LogInformation("GetISPAddressFromBackupAPIs -> ExternalResponseCount:{count}", ISPAddressChecks.Count);
            await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> ExternalResponseCount: {ISPAddressChecks.Count}");

            if (ISPAddressChecks.Count > 0)
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> More then one result:{count}", ISPAddressChecks.Count);
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> More then one result: {ISPAddressChecks.Count}");

                // Get the uniwue ISP adresses from the dictionary
                List<string>? uniqueAddresses = ISPAddressChecks?.Values?.Distinct()?.ToList()!;


                if (uniqueAddresses!.Count == 1)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> {count}x Same ISPAddress response:{ISPA}", ISPAddressChecks!.Count, StringHelpers.MakeISPAddressLogReady(uniqueAddresses[0]!));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> {ISPAddressChecks!.Count}x Same ISPAddress response: {StringHelpers.MakeISPAddressLogReady(uniqueAddresses[0]!)}");

                    // Update new ISP adress

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> External ISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress()));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> External ISPAddress BEFORE set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress())}");

                    _iSPAddressService.SetExternalISPAddress(uniqueAddresses[0]!);

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> External ISPAddress AFTER set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress()));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> External ISPAddress AFTER set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress())}");

                    // Copy the old ISP adress to that variable
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> CurrentISPAddress: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())}");

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> OldISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress()));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> OldISPAddress BEFORE set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress())}");

                    _iSPAddressService.SetOldISPAddress(_iSPAddressService.GetCurrentISPAddress());

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> OldISPAddress AFTER set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress()));
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> OldISPAddress AFTER set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetOldISPAddress())}");

                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> HeartBeatCheck:{heartbeat} GetServiceRequestCounter: {count1}, GetFailedISPRequestCounter: {count2} if(1 & 0 SendISPAddressChangedEmail)", heartBeatCheck, _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                    await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> HeartBeatCheck: {heartBeatCheck} GetServiceRequestCounter: {_counterService.GetServiceRequestCounter()}, GetFailedISPRequestCounter: {_counterService.GetFailedISPRequestCounter()} if(1 & 0 SendISPAddressChangedEmail)");

                    if (!heartBeatCheck && _counterService.GetServiceRequestCounter() != 1 && _counterService.GetFailedISPRequestCounter() != 0)
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE ClearCurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE ClearCurrentISPAddress: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())}");
                        _iSPAddressService.ClearCurrentISPAddress();

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER ClearCurrentISPAddress:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER ClearCurrentISPAddress: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())}");

                        await _emailService.SendISPAddressChangedEmail(_iSPAddressService.GetExternalISPAddress(), _iSPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes, _emailService.APIEmailDetails);

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> GetServiceRequestCounter not if(1): {count1}, GetFailedISPRequestCounter not if(0):{count2}", _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> GetServiceRequestCounter not if(1): {_counterService.GetServiceRequestCounter()}, GetFailedISPRequestCounter not if(0):{_counterService.GetFailedISPRequestCounter()}");
                    }
                    else if (!heartBeatCheck)
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> First setup");
                        await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressFromBackupAPIs -> First setup");

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> CurrentISPAddress BEFORE set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())}");

                        _iSPAddressService.SetCurrentISPAddress(_iSPAddressService.GetExternalISPAddress());

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER set:{ISP1} expected: {ISP2}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress()), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> CurrentISPAddress AFTER set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetCurrentISPAddress())} expected: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress())}");

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> NewISPAddress BEFORE set:{ISPA}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> NewISPAddress BEFORE set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())}");

                        _iSPAddressService.SetNewISPAddress(_iSPAddressService.GetExternalISPAddress());

                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> NewISPAddress AFTER set:{ISP1} expected: {ISP2}", StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress()), StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress()));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> NewISPAddress AFTER set: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetNewISPAddress())} expected: {StringHelpers.MakeISPAddressLogReady(_iSPAddressService.GetExternalISPAddress())}");
                    }
                }
                else
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> Different ISPAddresses returned");
                    await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressFromBackupAPIs -> Different ISPAddresses returned");

                    foreach (KeyValuePair<string, string> item in ISPAddressChecks!)
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> Different ISPAddresses returned -> URL: {Url} -> {ISP}", item.Key, StringHelpers.MakeISPAddressLogReady(item.Value));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> Different ISPAddresses returned -> URL: {item.Key} -> {StringHelpers.MakeISPAddressLogReady(item.Value)}");
                    }
                    await _emailService.SendDifferendISPAddressValuesEmail(ISPAddressChecks!, _iSPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes);
                }
            }
            else if (ISPAddressChecks.Count == 0)
            {
                _logger.LogInformation("GetISPAddressFromBackupAPIs -> No external results");
                await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressFromBackupAPIs -> No external results");

                await _emailService.SendNoISPAddressReturnedEmail(_iSPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes);
            }
        }
    }
}