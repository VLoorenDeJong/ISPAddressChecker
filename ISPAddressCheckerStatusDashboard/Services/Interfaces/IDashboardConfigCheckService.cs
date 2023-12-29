using ISPAddressChecker.Options;
using Microsoft.Extensions.Options;

namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IDashboardConfigCheckService
    {
        Task CheckApplicationConfig(IOptions<DashboardApplicationSettingsOptions> appSettings, IOptions<EmailSettingsOptions> emailSettings);
    }
}