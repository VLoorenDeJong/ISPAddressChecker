using ISPAddressChecker.Helpers;
using ISPAddressChecker.Options;
using ISPAddressChecker.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace ISPAddressChecker.Services
{
    public class ApplicationConfigCheckService : IApplicationConfigCheckService
    {
        public void CheckApplicationConfig(IOptions<ApplicationSettingsOptions> appSettings)
        {
            CheckAppsettingsVersionMatch(appSettings);
        }

        private void CheckAppsettingsVersionMatch(IOptions<ApplicationSettingsOptions> appSettings)
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
    }
}
