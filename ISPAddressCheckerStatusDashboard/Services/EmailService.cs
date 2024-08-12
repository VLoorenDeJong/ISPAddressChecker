using ISPAddressChecker.Options;
using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ISPAddressCheckerDashboard.Services
{
    public class EmailService : IDashboardEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly DashboardApplicationSettingsOptions _applicationSettingsOptions;
        private readonly EmailSettingsOptions _emailSettingsOptions;

        private MailMessage message = new MailMessage();
        private IRequestISPAddressService _ispService;

        public EmailService(
                              ILogger<EmailService> logger
                            , IOptions<DashboardApplicationSettingsOptions> applicationSettingsOptions
                            , IOptions<EmailSettingsOptions> emailSettingsOptions
            , IRequestISPAddressService ispService
            )
        {
            _ispService = ispService;
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;
            _emailSettingsOptions = emailSettingsOptions!.Value;

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

        public async Task SendConfigSuccessMail()
        {

            string apiURL = await _ispService.GetCHeckISPAddressEndpointURLAsync();

            string message = $@"<p>You have succesfully configured the ISPAddressDashboard.</p>"
                                + "<p><strong>This was fun! </strong></p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p>The folowing things were configured:</p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p><strong>Application settings:</strong></p>"
                                + $"<p>ShowSignalRTestClock: <strong>{_applicationSettingsOptions?.ShowSignalRTestClock}</strong></p>"
                                + $"<p>APIUrl: <strong>{apiURL}</strong></p>"
                                + $"<p>EmailCounterResetTimeInHours: <strong>{_applicationSettingsOptions?.EmailCounterResetTimeInHours}</strong></p>"
                                + $"<p>AppsettingsVersion: <strong>{_applicationSettingsOptions?.AppsettingsVersion}</strong></p>"
                                + $"<p>ExpectedAppsettingsVersion: <strong>{_applicationSettingsOptions?.ExpectedAppsettingsVersion}</strong></p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p><strong>Email settings:</strong></p>"
                                + $"<p>EmailFromAddress : <strong>{_emailSettingsOptions?.EmailFromAddress}</strong></p>"
                                + $"<p>EmailToAddress : <strong>{_emailSettingsOptions?.EmailToAddress}</strong></p>"
                                + $"<p>MailServer : <strong>{_emailSettingsOptions?.MailServer}</strong></p>"
                                + $"<p>userName: <strong>{_emailSettingsOptions?.UserName}</strong></p>"
                                + $"<p>password : <strong>*Your password*</strong></p>"
                                + $"<p>EnableSsl : <strong>{_emailSettingsOptions?.EnableSsl}</strong></p>"
                                + $"<p>SMTPPort : <strong>{_emailSettingsOptions?.SMTPPort}</strong></p>"
                                + $"<p>UseDefaultCredentials : <strong>{_emailSettingsOptions?.UseDefaultCredentials}</strong></p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p>I wish you a splendid rest of your day!</p>";

            string emailBody = CreateEmail(message);

            SendEmail("ISPAddressCheckerOptions - Configuration success!", emailBody);
        }
        public async Task SendConfigFailMail()
        {
           string apiURL =  await _ispService.GetCHeckISPAddressEndpointURLAsync();

            string message = $@"<p>Something is wrong with your configuration please check the setting below!</p>"
                                + "<p><strong>This was fun! </strong></p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p>The folowing things were configured:</p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p><strong>Application settings:</strong></p>"
                                + $"<p>ShowSignalRTestClock: <strong>{_applicationSettingsOptions?.ShowSignalRTestClock}</strong></p>"
                                + $"<p>APIUrl: <strong>{apiURL}</strong></p>"
                                + $"<p>EmailCounterResetTimeInHours: <strong>{_applicationSettingsOptions?.EmailCounterResetTimeInHours}</strong></p>"
                                + $"<p>AppsettingsVersion: <strong>{_applicationSettingsOptions?.AppsettingsVersion}</strong></p>"
                                + $"<p>ExpectedAppsettingsVersion: <strong>{_applicationSettingsOptions?.ExpectedAppsettingsVersion}</strong></p>"
                                + $@"<br />"
                                + $@"<br />"
                                + $"<p>Hope this was helpfull!</p>";

            string emailBody = CreateEmail(message);

            SendEmail("ISPAddressCheckerOptions - Configuration Error!", emailBody);

        }
        private void SendEmail(string subject, string emailBody)
        {
            if (_emailSettingsOptions is not null)
            {
                // Create a new SmtpClient object within a using block
                using (SmtpClient client = new SmtpClient())
                {
                    client.Host = _emailSettingsOptions?.MailServer!; ;
                    client.Port = _emailSettingsOptions!.SMTPPort;
                    client.UseDefaultCredentials = _emailSettingsOptions.UseDefaultCredentials;
                    client.Credentials = new NetworkCredential(_emailSettingsOptions?.UserName, _emailSettingsOptions?.Password);
                    client.EnableSsl = _emailSettingsOptions!.EnableSsl;

                    message.Subject = subject;
                    message.Body = emailBody;
                    message.IsBodyHtml = true;

                    message.To.Add(new MailAddress(_emailSettingsOptions!.EmailToAddress));

                    try
                    {
                        // Send the email message
                        client.Send(message);


                        _logger.LogInformation("SendEmail -> Sending: {subj}", subject);
                    }
                    catch (System.Net.Mail.SmtpException ex)
                    {
                        Type exceptionType = ex.GetType();
                        _logger.LogError("SendEmail -> Email account password might be wrong. Exception type: {exceptionType}  Message:{message}", exceptionType, ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Type exceptionType = ex.GetType();
                        _logger.LogError("SendEmail -> Something went wrong with sending the email. Exception type: {exceptionType} Message:{message}", exceptionType, ex.Message);
                    }
                }
            }
        }
    }
}
