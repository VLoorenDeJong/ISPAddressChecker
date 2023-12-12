using ISPAddressCheckerStatusDashboard.Options;
using Microsoft.Extensions.Options;

namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IApplicationConfigCheckService
    {
        Task CheckApplicationConfig(IOptions<Options.ApplicationSettingsOptions> appSettings, IOptions<Options.EmailSettingsOptions> emailSettings);
    }
}