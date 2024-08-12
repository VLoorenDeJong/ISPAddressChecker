using static ISPAddressChecker.Options.APIApplicationSettingsOptions;
using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Options;
using System.Net.Mail;
using System.Net;
using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Constants;

namespace ISPAddressChecker.Services
{
    public class EmailService : IAPIEmailService

    {
        private readonly ILogger _logger;
        private readonly APIEmailSettingsOptions _emailSettings;

        private readonly APIApplicationSettingsOptions _appSettings;

        private MailMessage message = new MailMessage();
        public SendEmailModel APIEmailDetails { get; private set; }

        private readonly ILogHubService _loghub;
        private readonly IStatusCounterService _statusCounter;

        private readonly string serviceName = nameof(EmailService);

        public EmailService(
                              ILogger<EmailService> logger
                            , IOptions<APIApplicationSettingsOptions> applicationSettingsOptions
                            , IOptions<APIEmailSettingsOptions> emailSettingsOptions
                            , ILogHubService loghub
                            , IStatusCounterService statusCounterService
                           )
        {
            _logger = logger;
            _appSettings = applicationSettingsOptions?.Value!;
            _emailSettings = emailSettingsOptions!.Value;

            APIEmailDetails = CreateInternalSendEmail();
            _loghub = loghub;

            _statusCounter = statusCounterService;
            CreateBasicMailMessage();
        }

        private void CreateBasicMailMessage()
        {
            // Set the sender, recipient, subject, and body of the message
            message.From = new MailAddress(_emailSettings.EmailFromAddress!);
            message.Priority = MailPriority.High;
        }

        private string CreateEmail(string emailMessage)
        {
            string outputMessage = "<html>"
                                     + "<head>"
                                        + "<style>"
                                             + "h1, h3, h4, h5, p { color: #666; font-family: Segoe UI; }"
                                             + "p { color: #666; font-family: Segoe UI; }"
                                         + "</style>"
                                     + "</head>"
                                     + "<body>"
                                     + $"{emailMessage}"
                                     + "</body>"
                                 + "</html>";

            return outputMessage;
        }

        private async Task<ActionReportModel> SendEmail(string subject, SendEmailModel sendEmailDetails, string emailBody)
        {
            ActionReportModel report = new(sendEmailDetails);



            //public bool Success { get; set; }
            //public string Message { get; set; } = string.Empty;

            if (_emailSettings is not null && sendEmailDetails is not null)
            {
                // Create a new SmtpClient object within a using block
                using (SmtpClient client = new SmtpClient())
                {
                    client.Host = _emailSettings.MailServer!; ;
                    client.Port = _emailSettings.SMTPPort;
                    client.UseDefaultCredentials = _emailSettings.UseDefaultCredentials;
                    client.Credentials = new NetworkCredential(_emailSettings?.UserName, _emailSettings?.Password);
                    client.EnableSsl = _emailSettings!.EnableSsl;

                    message.Subject = subject;
                    message.Body = emailBody;
                    message.IsBodyHtml = true;

                    if (string.IsNullOrWhiteSpace(sendEmailDetails.EmailAddress)) sendEmailDetails.EmailAddress = _emailSettings!.EmailToAddress!;
                    message.To.Add(new MailAddress(sendEmailDetails.EmailAddress));

                    try
                    {
                        // Send the email message
                       client.Send(message);


                        _logger.LogInformation("SendEmail -> Request Id: {id}, Sending: {subj}", sendEmailDetails.Id, subject);
                        await _loghub.SendLogInfoAsync(serviceName, $"RequestId: {sendEmailDetails.Id}, SendEmail -> Sending: {subject}");

                        report.Success = true;
                        report.Message = "E-mail has been send";
                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        Type exceptionType = ex.GetType();
                        _logger.LogError("SendEmail -> Email account password might be wrong. Exception type: {exceptionType}  Message:{message}", exceptionType, ex.Message);
                        await _loghub.SendLogErrorAsync(serviceName, $"SendEmail -> Email account password might be wrong. Exception type: {exceptionType}  Message:{ex.Message}");

                        report.Success = false;
                        report.Message = $"Sending E-mail failed";
                    }
                    catch (Exception ex)
                    {
                        Type exceptionType = ex.GetType();
                        _logger.LogError("SendEmail -> Request Id: {id}, Something went wrong with sending the email. Exception type: {exceptionType} Message:{message}", sendEmailDetails.Id, exceptionType, ex.Message);
                        await _loghub.SendLogErrorAsync(serviceName, $"RequestId: {sendEmailDetails.Id}, SendEmail -> Something went wrong with sending the email. Exception type: {exceptionType} Message:{ex.Message}");

                        report.Success = false;
                        report.Message = $"Sending E-mail failed";
                    }
                }
            }

            return report;
        }

        public async Task<ActionReportModel> SendHeartBeatEmail(IISPAddressCounterService counterService
                                                                , string oldISPAddress
                                                                , string currentISPAddress
                                                                , string newISPAddress
                                                                , Dictionary<string, string> externalISPCheckResults
                                                                , SendEmailModel sendEmailDetails
                                                                , TimeSpan uptime
                                                                )
        {

            string dashboardDetails = string.Empty;
            if (_appSettings.DashboardEnabled)
            {
                dashboardDetails =     $"<p><strong>Dasboard stats:</strong></p>"
                                     + $"<p>ISPAddress changed E-mail requests: <strong>{_statusCounter.GetISPISPAddressChangedEmailRequested()}</strong></p>"
                                     + $"<p>Heartbeat E-mail requests: <strong>{_statusCounter.GetISPHeartbeatEmailRequested()}</strong></p>"
                                     + $"<p></p>"
                                     ;
            }

            string backupAPIResults = string.Empty;
            int counter = 1;

            foreach (KeyValuePair<string, string> ISPAddressCheck in externalISPCheckResults!)
            {

                string ispReport = $"<p>{counter}: <a href = '{ISPAddressCheck.Key}'> {ISPAddressCheck.Key} </a> -> <strong>{ISPAddressCheck.Value}</strong></p>";
                backupAPIResults = $"{backupAPIResults} {ispReport}";
                counter++;
            }

            string message =      $"<p><strong>API stats:</strong></p>"
                                + $"<p>Uptime: <strong>{uptime.Days}</strong> days / <strong>{uptime.Hours}</strong> hours</p>"
                                + $"<p>API calls:<strong> {counterService.GetServiceRequestCounter()} </strong> / <strong> {counterService.GetServiceCheckCounter()}</strong></p>"
                                + $"<p>Internal API calls: <strong>{counterService.GetISPEndpointRequestsCounter()}</strong></p>"
                                + $"<p>External API calls: <strong>{counterService.GetExternalServiceUsekCounter()}</strong></p>"
                                + $@"<p>Current ISP: <strong> {currentISPAddress}</strong></p>"
                                + $"<p></p>"
                                + $"{dashboardDetails}"
                                + $"<p></p>"
                                + $"<p><strong>Backup API's:</strong></p>"
                                + $"{backupAPIResults}"
                                + $"<p></p>"
                                + $"<p><strong>ISP Values:</strong></p>"
                                + $@"<p>Old ISP: <strong> {oldISPAddress}</strong></p>"
                                + $@"<p>New ISP: <strong> {newISPAddress}</strong></p>"
                                + $"<p></p>"
                                + $"<p><strong>API config:</strong></p>"
                                + $"<p>ISPAddressCheckFrequencyInMinutes: <strong>{_appSettings?.ISPAddressCheckFrequencyInMinutes}</strong></p>"
                                + $"<p>DashboardEnabled: <strong>{_appSettings?.DashboardEnabled}</strong></p>"
                                + $"<p>DateTimeFormat: <strong>{_appSettings?.DateTimeFormat}</strong></p>"
                                + $"<p></p>"
                                + $@"<p><strong>This was fun! </strong></p>"
                                + $"<p>See you in {_emailSettings?.HeartbeatEmailIntervalDays} days ;)</p>"

                      ;

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendHeartBeatEmail -> Sending: SendHeartBeatEmail");

            return await SendEmail("ISPAddressCheckerAPI Heartbeat", sendEmailDetails, emailBody);
        }

        public async Task SendCounterDifferenceEmail(IISPAddressCounterService counterService)
        {
            string message = $"<p>The ISP check counters are out of sync.</p>"
                              + $"<p>requestCounter : <strong>{counterService.GetServiceRequestCounter()}</strong></p>"
                              + $"<p>checkCounter : <strong>{counterService.GetServiceCheckCounter()}</strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendCounterDifferenceEmail -> Sending: CounterDifferenceEmail");

            await SendEmail("ISPAddressCheckerAPI: counter difference", APIEmailDetails, emailBody);
        }

        public async Task SendConfigErrorMail(string errorMessage)
        {
            string emailBody = CreateEmail(errorMessage);


            _logger.LogError("SendConfigErrorMail -> Sending: ConfigErrorMail");
            await _loghub.SendLogErrorAsync(serviceName, $"SendConfigErrorMail -> Sending: ConfigErrorMail");

            await SendEmail("ISPAddressCheckerAPI: configuration error", APIEmailDetails, emailBody);
        }

        public async Task SendConfigSuccessMail(IISPAddressCounterService counterService)
        {
            string heartbeatMessage = _emailSettings.HeartbeatEmailEnabled ? $@"<p>Every week on <strong> {_emailSettings?.HeartbeatEmailDayOfWeek} </strong> at <strong> {_emailSettings?.HeartbeatEmailTimeOfDay} </strong> a status update E-mail will be send.</p>" : "<p>Heartbeat email: <strong> disabled</strong> </p>";

                string message = $@"<p>You have succesfully configured the ISPAddressAPI.</p>"
                                  + "<p><strong>This was fun! </strong></p>"
                                  + $"<p>I wish you a splendid rest of your day!</p>"
                                  + $@"<br />"
                                  + $@"<br />"
                                  + $"<p><strong>The folowing things were configured:</strong></p>"
                                  + $"<p>API endpoint URL:<a href = '{_appSettings?.APIEndpointURL}'> <strong>{_appSettings?.APIEndpointURL}</strong></a></p>"
                                  + $"<p>ISPAddressCheckFrequencyInMinutes: <strong>{_appSettings?.ISPAddressCheckFrequencyInMinutes}</strong></p>"                                  

                                  + $"{heartbeatMessage}"                                  

                                  + $"<p>DNSRecordHostProviderName: <strong>{_emailSettings?.DNSRecordHostProviderName}</strong></p>"
                                  + $"<p>DNSRecordHostProviderURL : <strong>{_emailSettings?.DNSRecordHostProviderURL}</strong></p>"
                                  + $"<p>EmailFromAddress : <strong>{_emailSettings?.EmailFromAddress}</strong></p>"
                                  + $"<p>EmailToAddress : <strong>{_emailSettings?.EmailToAddress}</strong></p>"
                                  + $"<p>EmailSubject : <strong>{_emailSettings?.EmailSubject}</strong></p>"
                                  + $"<p>MailServer : <strong>{_emailSettings?.MailServer}</strong></p>"
                                  + $"<p>userName: <strong>{_emailSettings?.UserName}</strong></p>"
                                  + $"<p>password : <strong>*Your password*</strong></p>"
                                  + $"<p>EnableSsl : <strong>{_emailSettings?.EnableSsl}</strong></p>"
                                  + $"<p>SMTPPort : <strong>{_emailSettings?.SMTPPort}</strong></p>"
                                  + $"<p>UseDefaultCredentials : <strong>{_emailSettings?.UseDefaultCredentials}</strong></p>"
                                  + $"<p>DateTimeFormat : <strong>{_appSettings?.DateTimeFormat}</strong></p>";
            // Write out the list of API's
            if (_appSettings?.BackupAPIS is not null)
            {
                foreach (string? backupAPI in _appSettings?.BackupAPIS!)
                {
                    message = $"{message} " +
                                $"<p>Backup API {_appSettings?.BackupAPIS.IndexOf(backupAPI)} : <a href = '{backupAPI}'> <strong>{backupAPI}</strong> </a></p>";
                }
            }
            // Finish the email body.
            message = $"{message} "
                       + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_appSettings?.DateTimeFormat)} </strong></p>"
                       + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                       + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                       + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong></p>"
                       + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                       + $"<p>A call is made every <strong> {_appSettings!.ISPAddressCheckFrequencyInMinutes} </strong>minutes</p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendConfigSuccessMail -> Sending: ConfigSuccessMail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendConfigSuccessMail -> Sending: ConfigSuccessMail");

            await SendEmail("ISPAddressCheckerAPI: Congratulations configuration succes!!", APIEmailDetails, emailBody);
        }

        public async Task SendConnectionReestablishedEmail(string newISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval)
        {
            string message = @$"<p>ISP adress has changed and I found my self again.</p>"
                            + @$"<p><strong> {newISPAddress} </strong> is your new ISP adress</p>"
                            + $"<p>API endpoint URL:<a href = '{_appSettings?.APIEndpointURL}'> <strong>{_appSettings?.APIEndpointURL}</strong></a></p>"
                            + "<p><strong>This is fun, hope it goes this well next time! </strong></p>"
                            + $"<p>I wish you a splendid rest of your day!</p>"
                            + $"<p>Your API</p>"
                            + $"<p><strong>Here are some statistics:</strong></p>"
                            + $"<p>A call is made every <strong> {interval} </strong>minutes</p>"
                            + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_appSettings?.DateTimeFormat)} </strong></p>"
                            + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong>(This counter is reset after this E-mail is send)</p>"
                            + $"<p>External API calls: <strong>{counterService.GetExternalServiceUsekCounter()}</strong></p>"
                            + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                            + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                            + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                            + $"<p>The old ISP adrdess was: {oldISPAddress}</p>"
                            + $"<p>API endpoint URL: <strong><a href{_appSettings?.APIEndpointURL}</strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendConnectionReestablishedEmail -> Sending: ConnectionReestablishedEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendConnectionReestablishedEmail -> Sending: ConnectionReestablishedEmail");

            await SendEmail("ISPAddressCheckerAPI:I found my self", APIEmailDetails, emailBody);
        }

        public async Task SendISPAPIHTTPExceptionEmail(string exceptionType, string exceptionMessage)
        {
            string message = $"<p>API Did not respond:</p>"
                           + $"<p>API endpoint URL:<a href = '{_appSettings?.APIEndpointURL}'> <strong>{_appSettings?.APIEndpointURL}</strong></a></p>"
                           + "<p>exceptionType:</p>"
                           + $"<p><strong>{exceptionType}</strong></p>"
                           + "<p>message:</p>"
                           + $"<p><strong>{exceptionMessage}<strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendISPAPIHTTPExceptionEmail -> Sending: ISPAPIHTTPExceptionEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendISPAPIHTTPExceptionEmail -> Sending: ISPAPIHTTPExceptionEmail");

            await SendEmail("ISPAddressCheckerAPI: API endpoint HTTP exception", APIEmailDetails, emailBody);
        }

        public async Task SendISPAPIExceptionEmail(string exceptionType, string exceptionMessage)
        {
            _logger.LogError("API Call error. Exceptiontype: {type} Message:{message}", exceptionType, exceptionMessage);
            await _loghub.SendLogErrorAsync(serviceName, $"API Call error. Exceptiontype: {exceptionType} Message:{exceptionMessage}");

            string message = $"<p>Exception fetching ISP address from API:</p>"
                           + $"<p>API endpoint URL:<a href = '{_appSettings?.APIEndpointURL}'> <strong>{_appSettings?.APIEndpointURL}</strong></a></p>"
                           + "<p>exceptionType:"
                           + $"<p><strong>{exceptionType}</strong></p>"
                           + "<p>message:"
                           + $"<p><strong>{exceptionMessage}<strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendISPAPIExceptionEmail -> Sending: ISPAPIExceptionEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendISPAPIExceptionEmail -> Sending: ISPAPIExceptionEmail");

            await SendEmail("ISPAddressCheckerAPI: API Call error", APIEmailDetails, emailBody);
        }

        public async Task SendExternalAPIHTTPExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage)
        {

            string message = $"<p>API Did not respond:</p>"
                            + $"<p><a href = '{APIUrl}'> <strong>{APIUrl}</strong> </a> </p>"
                            + "<p>exceptionType:</p>"
                            + $"<p><strong>{exceptionType}</strong></p>"
                            + "<p>message:</p>"
                            + $"<p><strong>{exceptionMessage}<strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendExternalAPIHTTPExceptionEmail -> Sending: ExternalAPIHTTPExceptionEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendExternalAPIHTTPExceptionEmail -> Sending: ExternalAPIHTTPExceptionEmail");

            await SendEmail("ISPAddressCheckerAPI: Backup API HTTP exception", APIEmailDetails, emailBody);
        }

        public async Task SendExternalAPIExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage)
        {
            string message = $"<p>Exception fetching ISP address from API:</p>"
                            + $"<p><a href = '{APIUrl}'> <strong>{APIUrl}</strong> </a> </p>"
                           + "<p>exceptionType:"
                           + $"<p><strong>{exceptionType}</strong></p>"
                           + "<p>message:"
                           + $"<p><strong>{exceptionMessage}<strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendExternalAPIExceptionEmail -> Sending: ExternalAPIExceptionEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendExternalAPIExceptionEmail -> Sending: ExternalAPIExceptionEmail");

            await SendEmail("ISPAddressCheckerAPI: API Call error", APIEmailDetails, emailBody);
        }

        public async Task<ActionReportModel> SendISPAddressChangedEmail(string externalISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval, SendEmailModel sendEmailDetails)
        {
            // hostingProviderText is the link to the hostprovider, id specified is shows the name
            string hostingProviderText = string.Equals(_emailSettings?.DNSRecordHostProviderName, StandardAppsettingsValues.DNSRecordHostProviderName, StringComparison.CurrentCultureIgnoreCase) ? _emailSettings?.DNSRecordHostProviderURL! : _emailSettings?.DNSRecordHostProviderName!;

            string hostingProviderLink = $"<p>Go to <a href = '{_emailSettings?.DNSRecordHostProviderURL}' target=\"_blank\"> <strong>{hostingProviderText}</strong> </a> to update the DNS record.</p>";
            string externalEmailLink = $"<p>Go to <a href = '{_appSettings?.APIEndpointURL}'target=\"_blank\"> <strong>Your provider link goes here</strong> </a> to update the DNS record.</p>";

            string emailLink = sendEmailDetails.EmailType != SendEmailTypeEnum.Internal ? externalEmailLink : hostingProviderLink;

            string message = @$"<p><strong> {externalISPAddress} </strong> is your new ISP adress</p>"
                              + $"{emailLink}"
                              + $"<p>External API calls: <strong>{counterService.GetExternalServiceUsekCounter()}</strong></p>"
                              + $"<p>I wish you a splendid rest of your day!</p>"
                              + $"<p>Your API</p>"
                              + $"<p><strong>Here are some statistics:</strong></p>"
                              + $"<p>API endpoint URL:<a href = '{_appSettings?.APIEndpointURL}'> <strong>{_appSettings?.APIEndpointURL}</strong></a></p>"
                              + $"<p>A call is made every <strong> {interval} </strong>minutes</p>"
                              + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_appSettings?.DateTimeFormat)} </strong></p>"
                              + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong></p>"
                              + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                              + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                              + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                              + $"<p>The old ISP adrdess was: <strong>{oldISPAddress}</strong></p>"
                              ;

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendISPAddressChangedEmail -> Sending: ISPAddressChangedEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"RequestId: {sendEmailDetails.Id}, SendISPAddressChangedEmail -> Sending: ISPAddressChangedEmail");

            return await SendEmail(_emailSettings?.EmailSubject!, sendEmailDetails, emailBody);
        }

        public async Task SendDifferendISPAddressValuesEmail(Dictionary<string, string> externalISPAddressChecks, string oldISPAddress, IISPAddressCounterService counterService, double interval)
        {
            string message = $@"<p><strong> Multiple </strong> ISP adresses returned</p>";

            foreach (KeyValuePair<string, string> ISPAddressCheck in externalISPAddressChecks!)
            {
                string ispReport = $"<p><a href = '{ISPAddressCheck.Key}'>{ISPAddressCheck.Key}</a> - <strong>{ISPAddressCheck.Value}</strong></p>";
                message = $"{message} {ispReport}";
            }

            message = $"{message}"
                        + "<p><strong>Best of luck solving this one!</strong></p>"
                        + $"<p>I wish you a splendid rest of your day!</p>"
                        + $"<p>Your API</p>"
                        + $"<p><strong>Here are some statistics:</strong></p>"
                        + $"<p>A call is made every <strong> {interval} </strong>minutes</p>"
                        + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_appSettings?.DateTimeFormat)} </strong></p>"
                        + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter} </strong></p>"
                        + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                        + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                        + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                        + $"<p>The old ISP adrdess was:<strong>{oldISPAddress}</strong></p>"
                        ;

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendDifferendISPAddressValuesEmail -> Sending: DifferendISPAddressValuesEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendDifferendISPAddressValuesEmail -> Sending: DifferendISPAddressValuesEmail");

            await SendEmail("ISPAddressCheckerAPI: multiple ISP adresses were returned", APIEmailDetails, emailBody);
        }

        public async Task SendNoISPAddressReturnedEmail(string oldISPAddress, IISPAddressCounterService counterService, double interval)
        {

            string message = @$"<p>No adresses were returned are there any exceptions?</p>"
                            + "<p><strong>Best of luck solving this one!</strong></p>"
                            + $"<p>I wish you a splendid rest of your day!</p>"
                            + $"<p>Your API</p>"
                            + $"<p><strong>Here are some statistics:</strong></p>"
                            + $"<p>A call is made every <strong> {interval} </strong>minutes</p>"
                            + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_appSettings.DateTimeFormat)} </strong></p>"
                            + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong></p>"
                            + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                            + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                            + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                            + $"<p>The old ISP adrdess was:<strong>{oldISPAddress}</strong></p>"
                            ;

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendNoISPAddressReturnedEmail -> Sending: NoISPAddressReturnedEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendNoISPAddressReturnedEmail -> Sending: NoISPAddressReturnedEmail");

            await SendEmail("ISPAddressCheckerAPI: No ISP adresses were returned", APIEmailDetails, emailBody);
        }

        private SendEmailModel CreateInternalSendEmail()
        {
            SendEmailModel output = new();

            if (Helpers.ValidationHelpers.EmailAddressIsValid(_emailSettings?.EmailToAddress))
            {
                output.EmailValidated = true;
                output.EmailAddress = _emailSettings!.EmailToAddress!;
                output.EmailType = SendEmailTypeEnum.Internal;
            }
            else
            {
                _logger.LogError("CreateInternalSendEmail -> EmailToAddress not valid, E-mail: {email}", _emailSettings?.EmailToAddress);
                throw new Exception($"CreateInternalSendEmail -> EmailToAddress not valid, E-mail: {_emailSettings?.EmailToAddress}");
            }

            return output;
        }
    }
}
