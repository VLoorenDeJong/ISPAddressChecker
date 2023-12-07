using ISPAddressChecker.Options;
using Microsoft.Extensions.Options;

namespace ISPAddressCheckerAPI.Services.Interfaces
{
    public interface IApplicationConfigCheckService
    {
        void CheckApplicationConfig(IOptions<ApplicationSettingsOptions> appSettings);
    }
}