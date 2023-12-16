using ISPAddressChecker.Options;
using Microsoft.Extensions.Options;

namespace ISPAddressChecker.Services.Interfaces
{
    public interface IApplicationConfigCheckService
    {
        void CheckApplicationConfig(IOptions<ApplicationSettingsOptions> appSettings);
    }
}