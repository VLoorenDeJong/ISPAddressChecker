using ISPAddressCheckerStatusDashboard.Options;
using ISPAddressCheckerStatusDashboard.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using static ISPAddressChecker.Options.ApplicationSettingsOptions;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public class ApplicationConfigCheckService : IApplicationConfigCheckService
    {
        private ILogger<ApplicationConfigCheckService>? _logger;
        private readonly IEmailService _emailService;

        public ApplicationConfigCheckService(ILogger<ApplicationConfigCheckService> logger, IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task CheckApplicationConfig(IOptions<Options.ApplicationSettingsOptions> appSettings, IOptions<Options.EmailSettingsOptions> emailSettings)
        {
            CheckAppsettingsVersionMatch(appSettings);
            if (CheckEmailSettings(emailSettings)) await _emailService.SendConfigSuccessMail();
        }

        private void CheckAppsettingsVersionMatch(IOptions<Options.ApplicationSettingsOptions> appSettings)
        {
            if (appSettings.Value.AppsettingsVersion == appSettings.Value.ExpectedAppsettingsVersion)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Startup => ConfigureServices => Version match! {appSettings.Value.ExpectedAppsettingsVersion}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Startup => ConfigureServices => Appsettings version issue!: expected: {appSettings.Value.ExpectedAppsettingsVersion} -> Current: {appSettings.Value.AppsettingsVersion}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Make sure the appsettings match");
                Console.ForegroundColor = ConsoleColor.White;
                throw new ArgumentException("Appsettings version mis match!");
            }
        }

        // ToDo: write methods for validating applicationsetting values

        private bool CheckEmailSettings(IOptions<Options.EmailSettingsOptions> emailSettings)
        {
            var settings = emailSettings.Value;

            bool mandatoryEmailConfigurationPassed = true;

            if (string.Equals(settings.MailServer, StandardAppsettingsValues.MailServer, StringComparison.CurrentCultureIgnoreCase))
            {
                string errorMessage = "appsettings: MailServer in appsettings not configured, this is for the mail you will recieve when the ISP adress is changed.";
                ThrowEmailConfigError(errorMessage, _logger);
                mandatoryEmailConfigurationPassed = false;
            }

            if (string.Equals(settings?.UserName, StandardAppsettingsValues.UserName, StringComparison.CurrentCultureIgnoreCase))
            {
                string errorMessage = "appsettings: UserName in appsettings not configured, this is for the mail you will recieve when the ISP adress is changed.";
                ThrowEmailConfigError(errorMessage, _logger);
                mandatoryEmailConfigurationPassed = false;
            }

            if (!settings!.UseDefaultCredentials && string.Equals(settings?.Password, StandardAppsettingsValues.Password, StringComparison.CurrentCultureIgnoreCase))
            {
                string errorMessage = "appsettings: Password in appsettings not configured, this is for the mail you will recieve when the ISP adress is changed.";
                ThrowEmailConfigError(errorMessage, _logger);
                mandatoryEmailConfigurationPassed = false;
            }

            if (string.Equals(settings?.EmailToAddress, StandardAppsettingsValues.EmailToAddress, StringComparison.CurrentCultureIgnoreCase) || !EmailAddressIsValid(settings?.EmailToAddress))
            {
                string errorMessage = $"appsettings: EmailToAddress: {settings?.EmailToAddress} in appsettings not confugured correctly";
                ThrowEmailConfigError(errorMessage, _logger);
                mandatoryEmailConfigurationPassed = false;
            }

            if (string.Equals(settings?.EmailFromAddress, StandardAppsettingsValues.EmailFromAddress, StringComparison.CurrentCultureIgnoreCase) || !EmailAddressIsValid(settings?.EmailFromAddress))
            {
                string errorMessage = $"appsettings: EmailFromAddress: {settings?.EmailFromAddress} in appsettings not confugured correctly";
                ThrowEmailConfigError(errorMessage, _logger);
                mandatoryEmailConfigurationPassed = false;
            }

            return mandatoryEmailConfigurationPassed;
        }

        private static void ThrowEmailConfigError(string errorMessage, ILogger? logger)
        {
            logger?.LogInformation(errorMessage);
            Console.WriteLine(errorMessage);
            throw new Exception(errorMessage);
        }


        public static bool EmailAddressIsValid(string? emailAddressToValidate)
        {
            bool isVallid = true;

            if (!string.IsNullOrWhiteSpace(emailAddressToValidate))
            {
                isVallid = Regex.IsMatch(emailAddressToValidate!, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            }

            // ToDo: Check validation it fails correct emailadresses?

            return isVallid;
        }
    }
}
