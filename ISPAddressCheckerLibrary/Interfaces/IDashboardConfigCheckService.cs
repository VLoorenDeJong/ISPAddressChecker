using Microsoft.Extensions.Options;

namespace ISPAddressChecker.Interfaces
{
    public interface IDashboardConfigCheckService
    {
        Task CheckApplicationConfig(IOptions<Options.DashboardApplicationSettingsOptions> appSettings, IOptions<Options.EmailSettingsOptions> emailSettings);
    }
}