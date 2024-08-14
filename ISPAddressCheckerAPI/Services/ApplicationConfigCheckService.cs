using ISPAddressChecker.Helpers;
using ISPAddressChecker.Options;
using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Models.Constants;
using ISPAddressChecker.Models;
using System.Text.RegularExpressions;

namespace ISPAddressCheckerAPI.Services
{
    public class ApplicationConfigCheckService : IAPIConfigCheckService
    {
        public void CheckApplicationConfig(IOptions<APIApplicationSettingsOptions> appSettings)
        {
            CheckAppsettingsVersionMatch(appSettings);
        }

        private void CheckAppsettingsVersionMatch(IOptions<APIApplicationSettingsOptions> appSettings)
        {
            if (appSettings.Value.AppsettingsVersion == appSettings?.Value?.ExpectedAppsettingsVersion)
            {

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Startup => ConfigureServices => Version match! {appSettings.Value.ExpectedAppsettingsVersion}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"Startup => ConfigureServices => Appsettings version issue!: expected: {appSettings?.Value?.ExpectedAppsettingsVersion} -> Current: {appSettings?.Value?.AppsettingsVersion}");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Make sure the appsettings match");
                Console.ForegroundColor = ConsoleColor.White;
                throw new ArgumentException("Appsettings version mis match!");
            }
        }

        public bool MandatoryConfigurationChecks(EmailSettingsOptions emailSettingsOptions, APIApplicationSettingsOptions applicationSettingOptions, ILogger logger)
        {
            bool MandatoryConfigurationPassed = true;

            if (string.Equals(emailSettingsOptions?.MailServer, StandardAppsettingsValues.MailServer, StringComparison.CurrentCultureIgnoreCase))
            {
                MandatoryConfigurationPassed = false;
                string errorMessage = "appsettings: MailServer in appsettings not configured, this is for the mail you will recieve when the ISP adress is changed.";

                ThrowEmailConfigError(errorMessage, logger);
            }


            if (string.Equals(emailSettingsOptions?.UserName, StandardAppsettingsValues.UserName, StringComparison.CurrentCultureIgnoreCase))
            {
                MandatoryConfigurationPassed = false;
                string errorMessage = "appsettings: UserName in appsettings not configured, this is for the mail you will recieve when the ISP adress is changed.";


                ThrowEmailConfigError(errorMessage, logger);
            }


            if (!emailSettingsOptions!.UseDefaultCredentials && string.Equals(emailSettingsOptions?.Password, StandardAppsettingsValues.Password, StringComparison.CurrentCultureIgnoreCase))
            {
                MandatoryConfigurationPassed = false;
                string errorMessage = "appsettings: Password in appsettings not configured, this is for the mail you will recieve when the ISP adress is changed.";

                ThrowEmailConfigError(errorMessage, logger);
            }


            if (string.Equals(emailSettingsOptions?.EmailToAddress, StandardAppsettingsValues.EmailToAddress, StringComparison.CurrentCultureIgnoreCase) || !ValidationHelpers.EmailAddressIsValid(emailSettingsOptions?.EmailToAddress))
            {
                MandatoryConfigurationPassed = false;
                string errorMessage = $"appsettings: EmailToAddress: {emailSettingsOptions?.EmailToAddress} in appsettings not confugured correctly";

                ThrowEmailConfigError(errorMessage, logger);
            }


            if (string.Equals(emailSettingsOptions?.EmailFromAddress, StandardAppsettingsValues.EmailFromAddress, StringComparison.CurrentCultureIgnoreCase) || !ValidationHelpers.EmailAddressIsValid(emailSettingsOptions?.EmailFromAddress))
            {
                MandatoryConfigurationPassed = false;
                string errorMessage = $"appsettings: EmailFromAddress: {emailSettingsOptions?.EmailFromAddress} in appsettings not confugured correctly";

                ThrowEmailConfigError(errorMessage, logger);
            }

            if (applicationSettingOptions?.BackupAPIS is null || BackupAPIUrlError(applicationSettingOptions?.BackupAPIS, logger))
            {

                MandatoryConfigurationPassed = false;
                string errorMessage = "appsettings: BackupAPIs in appsettings not confugured, this is for checking for you ISP adress when the ISP adress is changed.";

                ThrowEmailConfigError(errorMessage, logger);
            }

            return MandatoryConfigurationPassed;
        }

        public ConfigErrorReportModel DefaultSettingsCheck(APIEmailSettingsOptions emailSettingsOptions, APIApplicationSettingsOptions applicationSettingsOptions, ILogger logger)
        {
            ConfigErrorReportModel report = new();

            report.ChecksPassed = true;

            if (string.Equals(applicationSettingsOptions?.APIEndpointURL, StandardAppsettingsValues.APIEndpointURL, StringComparison.CurrentCultureIgnoreCase))
            {
                report.ChecksPassed = false;

                string errorMessage = $"<p><h5><strong>appsettings:</strong></h5></p>"
                                    + $"<p>The <strong>APIEndpointURL</strong> in appsettings is not changed</p>"
                                    + "<p>The application can not check your ISP address.</p>";

                ReportConfigError(errorMessage, logger);
                report.ErrorMessage = report.ErrorMessage + errorMessage;
            }


            if (string.Equals(emailSettingsOptions?.DNSRecordHostProviderURL, StandardAppsettingsValues.DNSRecordHostProviderURL, StringComparison.CurrentCultureIgnoreCase))
            {
                report.ChecksPassed = false;

                string errorMessage = $"<p><h5><strong>appsettings:</strong></h5></p>"
                                    + $"<p>The <strong> DNSRecordProviderURL </strong> in appsettings is not changed</p>"
                                    + "<p>This link will be in your email when your ISP address is changed.</p>";

                ReportConfigError(errorMessage, logger);
                report.ErrorMessage = report.ErrorMessage + errorMessage;
            }

            if (string.Equals(emailSettingsOptions?.EmailSubject, StandardAppsettingsValues.EmailSubject, StringComparison.CurrentCultureIgnoreCase))
            {
                report.ChecksPassed = false;

                string errorMessage = $"<p><h5><strong>appsettings:</strong></h5></p>"
                                    + $"<p>The <strong> EmailSubject </strong> in appsettings is not changed</p>"
                                    + "<p>this is for the mail you will recieve when the ISP adress is changed.</p>";

                ReportConfigError(errorMessage, logger);
                report.ErrorMessage = report.ErrorMessage + errorMessage;
            }

            if (emailSettingsOptions?.HeartbeatEmailIntervalDays == 0)
            {
                report.ChecksPassed = false;

                string errorMessage = $"<p><h5><strong>appsettings:</strong></h5></p>"
                                    + $"<p>The <strong> HeartbeatEmailIntervalDays </strong> in appsettings is not set</p>";

                ReportConfigError(errorMessage, logger);
                report.ErrorMessage = report.ErrorMessage + errorMessage;
            }

            return report;
        }

        private bool BackupAPIUrlError(List<string?>? backupAPIURLs, ILogger logger)
        {
            bool urlConfigError = false;

            if (backupAPIURLs is not null)
            {
                foreach (string? APIUrl in backupAPIURLs)
                {
                    if (string.IsNullOrWhiteSpace(APIUrl) || !Regex.IsMatch(APIUrl, @"^https?:\/\/[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$"))
                    {
                        urlConfigError = true;
                        string errorMessage = $"appsettings: Backup API URL not correct, index: {backupAPIURLs.IndexOf(APIUrl)}, URL: {APIUrl}";
                        ThrowEmailConfigError(errorMessage, logger);
                    }
                }
            }

            return urlConfigError;
        }

        private void ReportConfigError(string errorMessage, ILogger logger)
        {
            logger.LogCritical(errorMessage);
        }

        private void ThrowEmailConfigError(string errorMessage, ILogger logger)
        {
            logger.LogCritical(errorMessage);
            throw new Exception(errorMessage);
        }
    }
}
