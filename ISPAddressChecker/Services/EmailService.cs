using static ISPAddressChecker.Options.ApplicationSettingsOptions;
using ISPAddressChecker.Services.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Options;
using System.Net.Mail;
using System.Net;
using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Enums;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace ISPAddressChecker.Services
{
    //Todo: add the log hub to send the logs to the client if EnableDashboardAccess is true
    public class EmailService : IEmailService

    {
        private readonly ILogger _logger;
        private readonly EmailSettingsOptions _emailSettingsOptions;

        private readonly ApplicationSettingsOptions _applicationSettingsOptions;

        private MailMessage message = new MailMessage();
        public SendEmailModel APIEmailDetails { get; private set; }

        private readonly ILogHubService _loghub;
        private readonly string serviceName = nameof(EmailService);

        public EmailService(
                              ILogger<CheckISPAddressService> logger
                            , IOptions<ApplicationSettingsOptions> applicationSettingsOptions
                            , IOptions<EmailSettingsOptions> emailSettingsOptions
                            , ILogHubService loghub
                           )
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;
            _emailSettingsOptions = emailSettingsOptions!.Value;

            APIEmailDetails = CreateInternalSendEmail();
            _loghub = loghub;

            CreateBasicMailMessage();
        }

        private void CreateBasicMailMessage()
        {
            // Set the sender, recipient, subject, and body of the message
            message.From = new MailAddress(_emailSettingsOptions.EmailFromAddress!);
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

            if (_emailSettingsOptions is not null && sendEmailDetails is not null)
            {
                // Create a new SmtpClient object within a using block
                using (SmtpClient client = new SmtpClient())
                {
                    client.Host = _emailSettingsOptions.MailServer!; ;
                    client.Port = _emailSettingsOptions.SMTPPort;
                    client.UseDefaultCredentials = _emailSettingsOptions.UseDefaultCredentials;
                    client.Credentials = new NetworkCredential(_emailSettingsOptions?.UserName, _emailSettingsOptions?.Password);
                    client.EnableSsl = _emailSettingsOptions!.EnableSsl;

                    message.Subject = subject;
                    message.Body = emailBody;
                    message.IsBodyHtml = true;

                    if (string.IsNullOrWhiteSpace(sendEmailDetails.EmailAddress)) sendEmailDetails.EmailAddress = _emailSettingsOptions!.EmailToAddress!;
                    message.To.Add(new MailAddress(sendEmailDetails.EmailAddress));

                    try
                    {
                        // Send the email message
                        // ToDo: enable sending emails
                        client.Send(message);


                        _logger.LogInformation("SendEmail -> Request Id: {id}, Sending: {subj}", sendEmailDetails.Id, subject);
                        await _loghub.SendLogInfoAsync(serviceName, $"SendEmail -> Request Id: {sendEmailDetails.Id}, Sending: {subject}");

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
                        await _loghub.SendLogErrorAsync(serviceName, $"SendEmail -> Request Id: {sendEmailDetails.Id}, Something went wrong with sending the email. Exception type: {exceptionType} Message:{ex.Message}");

                        report.Success = false;
                        report.Message = $"Sending E-mail failed";
                    }
                }
            }

            return report;
        }

        public async Task<ActionReportModel> SendHeartBeatEmail(IISPAddressCounterService counterService, string oldISPAddress, string currentISPAddress, string newISPAddress, Dictionary<string, string> externalISPCheckResults, SendEmailModel sendEmailDetails)
        {

            string message = $@"<p><strong>This was fun! </strong></p>"
                                 + $"<p>API calls:<strong> {counterService.GetServiceRequestCounter()}</strong></p>"
                                 + $"<p>API call check: <strong>{counterService.GetServiceCheckCounter()}</strong></p>"
                                 + $"<p>Internal API calls: <strong>{counterService.GetISPEndpointRequestsCounter()}</strong></p>"
                                 + $"<p>External API calls: <strong>{counterService.GetExternalServiceUsekCounter()}</strong></p>"
                                 + $@"<p>Current ISP: <strong> {currentISPAddress}</strong></p>";
            foreach (KeyValuePair<string, string> ISPAddressCheck in externalISPCheckResults!)
            {

                string ispReport = $"<p>Backup API: <a href = '{ISPAddressCheck.Key}'> {ISPAddressCheck.Key} </a> -> <strong>{ISPAddressCheck.Value}</strong></p>";
                message = $"{message} {ispReport}";
            }
            message = $"{message} <p>ISPAddressCheckFrequencyInMinutes: <strong>{_applicationSettingsOptions?.ISPAddressCheckFrequencyInMinutes}</strong></p>"
                      + $"<p>API endpoint URL:<a href = '{_applicationSettingsOptions?.APIEndpointURL}'> <strong>{_applicationSettingsOptions?.APIEndpointURL}</strong></a></p>"
                      + $@"<p>Old ISP: <strong> {oldISPAddress}</strong></p>"
                      + $@"<p>New ISP: <strong> {newISPAddress}</strong></p>"
                      + $"<p>See you in {_emailSettingsOptions?.HeartbeatEmailIntervalDays} days ;)</p>"
                      + $"<p></p>"
                      + $"<p>I wish you a splendid rest of your day!</p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendHeartBeatEmail -> Sending: SendHeartBeatEmail");

            return await SendEmail("ISPAddressCheckerAPI update", sendEmailDetails, emailBody);
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
            string heartbeatMessage = _emailSettingsOptions.HeartbeatEmailEnabled ? $@"<p>Every week on <strong> {_emailSettingsOptions?.HeartbeatEmailDayOfWeek} </strong> at <strong> {_emailSettingsOptions?.HeartbeatEmailTimeOfDay} </strong> a status update E-mail will be send.</p>" : "<p>Heartbeat email: <strong> disabled</strong> </p>";

                string message = $@"<p>You have succesfully configured the ISPAddressAPI.</p>"
                                  + "<p><strong>This was fun! </strong></p>"
                                  + $"<p>I wish you a splendid rest of your day!</p>"
                                  + $@"<br />"
                                  + $@"<br />"
                                  + $"<p><strong>The folowing things were configured:</strong></p>"
                                  + $"<p>API endpoint URL:<a href = '{_applicationSettingsOptions?.APIEndpointURL}'> <strong>{_applicationSettingsOptions?.APIEndpointURL}</strong></a></p>"
                                  + $"<p>ISPAddressCheckFrequencyInMinutes: <strong>{_applicationSettingsOptions?.ISPAddressCheckFrequencyInMinutes}</strong></p>"                                  

                                  + $"{heartbeatMessage}"                                  

                                  + $"<p>DNSRecordHostProviderName: <strong>{_emailSettingsOptions?.DNSRecordHostProviderName}</strong></p>"
                                  + $"<p>DNSRecordHostProviderURL : <strong>{_emailSettingsOptions?.DNSRecordHostProviderURL}</strong></p>"
                                  + $"<p>EmailFromAddress : <strong>{_emailSettingsOptions?.EmailFromAddress}</strong></p>"
                                  + $"<p>EmailToAddress : <strong>{_emailSettingsOptions?.EmailToAddress}</strong></p>"
                                  + $"<p>EmailSubject : <strong>{_emailSettingsOptions?.EmailSubject}</strong></p>"
                                  + $"<p>MailServer : <strong>{_emailSettingsOptions?.MailServer}</strong></p>"
                                  + $"<p>userName: <strong>{_emailSettingsOptions?.UserName}</strong></p>"
                                  + $"<p>password : <strong>*Your password*</strong></p>"
                                  + $"<p>EnableSsl : <strong>{_emailSettingsOptions?.EnableSsl}</strong></p>"
                                  + $"<p>SMTPPort : <strong>{_emailSettingsOptions?.SMTPPort}</strong></p>"
                                  + $"<p>UseDefaultCredentials : <strong>{_emailSettingsOptions?.UseDefaultCredentials}</strong></p>"
                                  + $"<p>DateTimeFormat : <strong>{_applicationSettingsOptions?.DateTimeFormat}</strong></p>";
            // Write out the list of API's
            if (_applicationSettingsOptions?.BackupAPIS is not null)
            {
                foreach (string? backupAPI in _applicationSettingsOptions?.BackupAPIS!)
                {
                    message = $"{message} " +
                                $"<p>Backup API {_applicationSettingsOptions?.BackupAPIS.IndexOf(backupAPI)} : <a href = '{backupAPI}'> <strong>{backupAPI}</strong> </a></p>";
                }
            }
            // Finish the email body.
            message = $"{message} "
                       + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_applicationSettingsOptions?.DateTimeFormat)} </strong></p>"
                       + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                       + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                       + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong></p>"
                       + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                       + $"<p>A call is made every <strong> {_applicationSettingsOptions!.ISPAddressCheckFrequencyInMinutes} </strong>minutes</p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendConfigSuccessMail -> Sending: ConfigSuccessMail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendConfigSuccessMail -> Sending: ConfigSuccessMail");

            await SendEmail("ISPAddressCheckerAPI: Congratulations configuration succes!!", APIEmailDetails, emailBody);
        }

        public async Task SendConnectionReestablishedEmail(string newISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval)
        {
            string message = @$"<p>ISP adress has changed and I found my self again.</p>"
                            + @$"<p><strong> {newISPAddress} </strong> is your new ISP adress</p>"
                            + $"<p>API endpoint URL:<a href = '{_applicationSettingsOptions?.APIEndpointURL}'> <strong>{_applicationSettingsOptions?.APIEndpointURL}</strong></a></p>"
                            + "<p><strong>This is fun, hope it goes this well next time! </strong></p>"
                            + $"<p>I wish you a splendid rest of your day!</p>"
                            + $"<p>Your API</p>"
                            + $"<p><strong>Here are some statistics:</strong></p>"
                            + $"<p>A call is made every <strong> {interval} </strong>minutes</p>"
                            + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_applicationSettingsOptions?.DateTimeFormat)} </strong></p>"
                            + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong>(This counter is reset after this E-mail is send)</p>"
                            + $"<p>External API calls: <strong>{counterService.GetExternalServiceUsekCounter()}</strong></p>"
                            + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                            + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                            + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                            + $"<p>The old ISP adrdess was: {oldISPAddress}</p>"
                            + $"<p>API endpoint URL: <strong><a href{_applicationSettingsOptions?.APIEndpointURL}</strong></p>";

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendConnectionReestablishedEmail -> Sending: ConnectionReestablishedEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendConnectionReestablishedEmail -> Sending: ConnectionReestablishedEmail");

            await SendEmail("ISPAddressCheckerAPI:I found my self", APIEmailDetails, emailBody);
        }

        public async Task SendISPAPIHTTPExceptionEmail(string exceptionType, string exceptionMessage)
        {
            string message = $"<p>API Did not respond:</p>"
                           + $"<p>API endpoint URL:<a href = '{_applicationSettingsOptions?.APIEndpointURL}'> <strong>{_applicationSettingsOptions?.APIEndpointURL}</strong></a></p>"
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
                           + $"<p>API endpoint URL:<a href = '{_applicationSettingsOptions?.APIEndpointURL}'> <strong>{_applicationSettingsOptions?.APIEndpointURL}</strong></a></p>"
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
            string hostingProviderText = string.Equals(_emailSettingsOptions?.DNSRecordHostProviderName, StandardAppsettingsValues.DNSRecordHostProviderName, StringComparison.CurrentCultureIgnoreCase) ? _emailSettingsOptions?.DNSRecordHostProviderURL! : _emailSettingsOptions?.DNSRecordHostProviderName!;

            string hostingProviderLink = $"<p>Go to <a href = '{_emailSettingsOptions?.DNSRecordHostProviderURL}' target=\"_blank\"> <strong>{hostingProviderText}</strong> </a> to update the DNS record.</p>";
            string externalEmailLink = $"<p>Go to <a href = '{_applicationSettingsOptions?.APIEndpointURL}'target=\"_blank\"> <strong>Your provider link goes here</strong> </a> to update the DNS record.</p>";

            string emailLink = sendEmailDetails.EmailType != SendEmailTypeEnum.Internal ? externalEmailLink : hostingProviderLink;

            string message = @$"<p><strong> {externalISPAddress} </strong> is your new ISP adress</p>"
                              + $"{emailLink}"
                              + $"<p>External API calls: <strong>{counterService.GetExternalServiceUsekCounter()}</strong></p>"
                              + $"<p>I wish you a splendid rest of your day!</p>"
                              + $"<p>Your API</p>"
                              + $"<p><strong>Here are some statistics:</strong></p>"
                              + $"<p>API endpoint URL:<a href = '{_applicationSettingsOptions?.APIEndpointURL}'> <strong>{_applicationSettingsOptions?.APIEndpointURL}</strong></a></p>"
                              + $"<p>A call is made every <strong> {interval} </strong>minutes</p>"
                              + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_applicationSettingsOptions?.DateTimeFormat)} </strong></p>"
                              + $"<p>Failed attempts counter: <strong> {counterService.GetFailedISPRequestCounter()} </strong></p>"
                              + $"<p>API Calls: <strong> {counterService.GetServiceRequestCounter()} </strong></p>"
                              + $"<p>Script runs: <strong> {counterService.GetServiceCheckCounter()} </strong></p>"
                              + $"<p>Endpoint calls: <strong> {counterService.GetISPEndpointRequestsCounter()} </strong></p>"
                              + $"<p>The old ISP adrdess was: <strong>{oldISPAddress}</strong></p>"
                              ;

            string emailBody = CreateEmail(message);

            _logger.LogInformation("SendISPAddressChangedEmail -> Sending: ISPAddressChangedEmail");
            await _loghub.SendLogInfoAsync(serviceName, $"SendISPAddressChangedEmail -> Sending: ISPAddressChangedEmail");

            return await SendEmail(_emailSettingsOptions?.EmailSubject!, sendEmailDetails, emailBody);
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
                        + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_applicationSettingsOptions?.DateTimeFormat)} </strong></p>"
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
                            + $"<p>The time of this check: <strong> {DateTime.Now.ToString(_applicationSettingsOptions.DateTimeFormat)} </strong></p>"
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

            if (Helpers.ConfigHelpers.EmailAddressIsValid(_emailSettingsOptions?.EmailToAddress))
            {
                output.EmailValidated = true;
                output.EmailAddress = _emailSettingsOptions!.EmailToAddress!;
                output.EmailType = SendEmailTypeEnum.Internal;
            }
            else
            {
                _logger.LogError("CreateInternalSendEmail -> EmailToAddress not valid, E-mail: {email}", _emailSettingsOptions?.EmailToAddress);
                throw new Exception($"CreateInternalSendEmail -> EmailToAddress not valid, E-mail: {_emailSettingsOptions?.EmailToAddress}");
            }

            return output;
        }
    }
}
