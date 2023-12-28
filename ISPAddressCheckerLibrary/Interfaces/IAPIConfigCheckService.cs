using ISPAddressChecker.Options;
using Microsoft.Extensions.Options;

namespace ISPAddressChecker.Interfaces
{
    public interface IAPIConfigCheckService
    {
        void CheckApplicationConfig(IOptions<APIApplicationSettingsOptions> appSettings);
    }
}