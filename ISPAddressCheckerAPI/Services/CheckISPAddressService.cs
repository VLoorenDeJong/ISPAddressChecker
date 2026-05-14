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

                        _logger.LogInformation(ex, "GetISPAddressAsync -> HttpStatusCode.ServiceUnavailable, starting external calls");
                        await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressAsync -> HttpStatusCode.ServiceUnavailable, starting external calls");

                        await GetISPAddressFromBackupAPIs(false);
                    }
                    else
                    {
                        Type exceptionType = ex.GetType();

                        _logger.LogError(ex, "GetISPAddressAsync -> API Call HTTP exception. Exceptiontype: {type} Message:{message}", exceptionType, ex.Message);
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

                    _logger.LogError(ex, "GetISPAddressAsync -> API Call general Exception. Exceptiontype: {type} Message:{message}", exceptionType, ex.Message);
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

                        // Try to extract IPv4 first, then fall back to IPv6
                        Match match = Regex.Match(ISPAddress, @"\b(?:\d{1,3}\.){3}\d{1,3}\b");
                        if (match.Success)
                        {
                            ISPAddress = match.Value;
                        }
                        else
                        {
                            Match ipv6Match = Regex.Match(ISPAddress, @"(?:[0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}");
                            if (ipv6Match.Success)
                            {
                                ISPAddress = ipv6Match.Value;
                            }
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

                        _logger.LogError(ex, "GetISPAddressFromBackupAPIs -> API Call HttpRequestException -> URL:{APIUrl}. Exceptiontype: {type} Message:{message}", APIUrl, exceptionType, ex.Message);
                        await _logHub.SendLogErrorAsync(serviceName, $"GetISPAddressFromBackupAPIs -> API Call HttpRequestException -> URL:{APIUrl}. Exceptiontype: {exceptionType}, Message:{ex.Message}");

                        await _emailService.SendExternalAPIHTTPExceptionEmail(APIUrl!, exceptionType.Name, ex.Message);

                    }
                    catch (Exception ex)
                    {

                        Type exceptionType = ex.GetType();

                        _logger.LogError(ex, "GetISPAddressFromBackupAPIs -> API Call Exception -> URL:{APIUrl}. Exceptiontype: {type} Message:{message}", APIUrl, exceptionType, ex.Message);
                        await _logHub.SendLogErrorAsync(serviceName, $"GetISPAddressFromBackupAPIs -> API Call Exception -> URL:{APIUrl}. Exceptiontype: {exceptionType}, Message:{ex.Message}");

                        await _emailService.SendExternalAPIExceptionEmail(APIUrl!, exceptionType.Name, ex.Message);
                    }
                }
            }

            _logger.LogInformation("GetISPAddressFromBackupAPIs -> ExternalResponseCount:{count}", ISPAddressChecks.Count);
            await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> ExternalResponseCount: {ISPAddressChecks.Count}");

            if (ISPAddressChecks.Count > 0)
            {
                // Split results by IP version so mixed IPv4/IPv6 responses are handled independently
                var ipv4Checks = ISPAddressChecks
                    .Where(kvp => System.Net.IPAddress.TryParse(kvp.Value.Trim(), out var a)
                                  && a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                var ipv6Checks = ISPAddressChecks
                    .Where(kvp => System.Net.IPAddress.TryParse(kvp.Value.Trim(), out var a)
                                  && a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> IPv4 results:{v4}, IPv6 results:{v6}", ipv4Checks.Count, ipv6Checks.Count);
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> IPv4 results: {ipv4Checks.Count}, IPv6 results: {ipv6Checks.Count}");

                // ── Process IPv4 ────────────────────────────────────────────────
                if (ipv4Checks.Count > 0)
                {
                    List<string> uniqueV4 = ipv4Checks.Values.Distinct().ToList();

                    if (uniqueV4.Count == 1)
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> {count}x Same IPv4 response:{ISPA}", ipv4Checks.Count, StringHelpers.MakeISPAddressLogReady(uniqueV4[0]));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> {ipv4Checks.Count}x Same IPv4 response: {StringHelpers.MakeISPAddressLogReady(uniqueV4[0])}");

                        _iSPAddressService.SetExternalISPAddress(uniqueV4[0]);
                        _iSPAddressService.SetOldISPAddress(_iSPAddressService.GetCurrentISPAddress());
                    }
                    else
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> Conflicting IPv4 addresses returned");
                        await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressFromBackupAPIs -> Conflicting IPv4 addresses returned");
                        await _emailService.SendDifferendISPAddressValuesEmail(ipv4Checks, _iSPAddressService.GetOldISPAddress(), _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes);
                    }
                }

                // ── Process IPv6 ────────────────────────────────────────────────
                if (ipv6Checks.Count > 0)
                {
                    List<string> uniqueV6 = ipv6Checks.Values.Distinct().ToList();

                    if (uniqueV6.Count == 1)
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> {count}x Same IPv6 response:{ISPA}", ipv6Checks.Count, StringHelpers.MakeISPAddressLogReady(uniqueV6[0]));
                        await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> {ipv6Checks.Count}x Same IPv6 response: {StringHelpers.MakeISPAddressLogReady(uniqueV6[0])}");

                        _iSPAddressService.SetExternalIPv6Address(uniqueV6[0]);
                        _iSPAddressService.SetOldIPv6Address(_iSPAddressService.GetCurrentIPv6Address());
                    }
                    else
                    {
                        _logger.LogInformation("GetISPAddressFromBackupAPIs -> Conflicting IPv6 addresses returned");
                        await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressFromBackupAPIs -> Conflicting IPv6 addresses returned");
                        await _emailService.SendDifferendISPAddressValuesEmail(ipv6Checks, _iSPAddressService.GetOldIPv6Address(), _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes);
                    }
                }

                // ── Detect changes ───────────────────────────────────────────────
                bool v4Changed = !string.IsNullOrEmpty(_iSPAddressService.GetExternalISPAddress())
                    && !string.Equals(_iSPAddressService.GetExternalISPAddress(), _iSPAddressService.GetCurrentISPAddress(), StringComparison.CurrentCultureIgnoreCase);

                bool v6Changed = !string.IsNullOrEmpty(_iSPAddressService.GetExternalIPv6Address())
                    && !string.Equals(_iSPAddressService.GetExternalIPv6Address(), _iSPAddressService.GetCurrentIPv6Address(), StringComparison.CurrentCultureIgnoreCase);

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> v4Changed:{v4}, v6Changed:{v6}", v4Changed, v6Changed);
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> v4Changed:{v4Changed}, v6Changed:{v6Changed}");

                _logger.LogInformation("GetISPAddressFromBackupAPIs -> HeartBeatCheck:{heartbeat} GetServiceRequestCounter:{count1}, GetFailedISPRequestCounter:{count2}", heartBeatCheck, _counterService.GetServiceRequestCounter(), _counterService.GetFailedISPRequestCounter());
                await _logHub.SendLogInfoAsync(serviceName, $"GetISPAddressFromBackupAPIs -> HeartBeatCheck:{heartBeatCheck} GetServiceRequestCounter:{_counterService.GetServiceRequestCounter()}, GetFailedISPRequestCounter:{_counterService.GetFailedISPRequestCounter()}");

                if (!heartBeatCheck && _counterService.GetServiceRequestCounter() != 1 && _counterService.GetFailedISPRequestCounter() != 0)
                {
                    if (v4Changed || v6Changed)
                    {
                        _iSPAddressService.ClearCurrentISPAddress();

                        await _emailService.SendISPAddressChangedEmail(
                            _iSPAddressService.GetExternalISPAddress(), _iSPAddressService.GetOldISPAddress(),
                            _iSPAddressService.GetExternalIPv6Address(), _iSPAddressService.GetOldIPv6Address(),
                            _counterService, _applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes, _emailService.APIEmailDetails);
                    }
                }
                else if (!heartBeatCheck)
                {
                    _logger.LogInformation("GetISPAddressFromBackupAPIs -> First setup");
                    await _logHub.SendLogInfoAsync(serviceName, "GetISPAddressFromBackupAPIs -> First setup");

                    if (!string.IsNullOrEmpty(_iSPAddressService.GetExternalISPAddress()))
                    {
                        _iSPAddressService.SetCurrentISPAddress(_iSPAddressService.GetExternalISPAddress());
                        _iSPAddressService.SetNewISPAddress(_iSPAddressService.GetExternalISPAddress());
                    }

                    if (!string.IsNullOrEmpty(_iSPAddressService.GetExternalIPv6Address()))
                    {
                        _iSPAddressService.SetCurrentIPv6Address(_iSPAddressService.GetExternalIPv6Address());
                        _iSPAddressService.SetNewIPv6Address(_iSPAddressService.GetExternalIPv6Address());
                    }
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