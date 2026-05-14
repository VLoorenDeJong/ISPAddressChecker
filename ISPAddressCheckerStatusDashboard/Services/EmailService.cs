using ISPAddressChecker.Options;
using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ISPAddressCheckerDashboard.Services
{
    public class EmailService : IDashboardEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly DashboardApplicationSettingsOptions _applicationSettingsOptions;
        private readonly EmailSettingsOptions _emailSettingsOptions;

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

            await SendEmail("ISPAddressCheckerOptions - Configuration success!", emailBody);
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

            await SendEmail("ISPAddressCheckerOptions - Configuration Error!", emailBody);

        }
        private async Task SendEmail(string subject, string emailBody)
        {
            if (_emailSettingsOptions is not null)
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(string.Empty, _emailSettingsOptions.EmailFromAddress!));
                mimeMessage.To.Add(new MailboxAddress(string.Empty, _emailSettingsOptions.EmailToAddress!));
                mimeMessage.Priority = MessagePriority.Urgent;
                mimeMessage.Subject = subject;
                mimeMessage.Body = new BodyBuilder { HtmlBody = emailBody }.ToMessageBody();

                try
                {
                    using var client = new SmtpClient();

                    var socketOptions = _emailSettingsOptions.SMTPPort == 465
                        ? SecureSocketOptions.SslOnConnect
                        : _emailSettingsOptions.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

                    await client.ConnectAsync(_emailSettingsOptions.MailServer!, _emailSettingsOptions.SMTPPort, socketOptions);

                    if (!_emailSettingsOptions.UseDefaultCredentials)
                        await client.AuthenticateAsync(_emailSettingsOptions.UserName!, _emailSettingsOptions.Password!);

                    await client.SendAsync(mimeMessage);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation("SendEmail -> Sending: {subj}", subject);
                }
                catch (AuthenticationException ex)
                {
                    _logger.LogError(ex, "SendEmail -> Authentication failed. Check username/password. Message: {message}", ex.Message);
                }
                catch (Exception ex)
                {
                    Type exceptionType = ex.GetType();
                    _logger.LogError(ex, "SendEmail -> Something went wrong with sending the email. Exception type: {exceptionType} Message:{message}", exceptionType, ex.Message);
                }
            }
        }
    }
}
