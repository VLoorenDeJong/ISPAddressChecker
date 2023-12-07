using Microsoft.Extensions.Options;

namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IApplicationConfigCheckService
    {
        void CheckApplicationConfig(IOptions<Options.ApplicationSettingsOptions> appSettings);
        void CheckApplicationConfig(IOptions<ISPAddressChecker.Options.ApplicationSettingsOptions> appSettings);
    }
}