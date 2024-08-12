using ISPAddressChecker.Models;
using ISPAddressChecker.Options;
using Microsoft.Extensions.Options;

namespace ISPAddressCheckerAPI.Services
{
    public interface IAPIConfigCheckService
    {
        void CheckApplicationConfig(IOptions<APIApplicationSettingsOptions> appSettings);
        ConfigErrorReportModel DefaultSettingsCheck(APIEmailSettingsOptions emailSettingsOptions, APIApplicationSettingsOptions applicationSettingsOptions, ILogger logger);
        bool MandatoryConfigurationChecks(EmailSettingsOptions emailSettingsOptions, APIApplicationSettingsOptions applicationSettingOptions, ILogger logger);
    }
}