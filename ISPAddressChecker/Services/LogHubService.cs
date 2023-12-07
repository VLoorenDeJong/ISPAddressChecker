using ISPAddressChecker.SignalRHubs.Interfaces;
using ISPAddressChecker.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Enums;
using ISPAddressChecker.Services.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Options;

namespace ISPAddressChecker.Services
{
    public class LogHubService : ILogHubService
    {

        private readonly IHubContext<LogHub, ILogHub> _logHub;
        private readonly ApplicationSettingsOptions _appSettings;

        public LogHubService(
                             IHubContext<LogHub, ILogHub> logHub
                             , IOptions<ApplicationSettingsOptions> appsettings
                            )
        {
            _logHub = logHub;
            _appSettings = appsettings.Value;
        }

        public async Task SendLogInfoAsync(string serviceName, string message)
        {
            if (_appSettings.EnableDashboardAccess)
            {
                var logEntry = new LogEntryModel(LogType.Information,
                                             serviceName,
                                             message);
                await _logHub.Clients.All.SendLogToClients(logEntry);
            }
        }

        public async Task SendLogWarningAsync(string serviceName, string message)
        {
            if (_appSettings.EnableDashboardAccess)
            {
                var logEntry = new LogEntryModel(LogType.Warning,
                                            serviceName,
                                            message);
                await _logHub.Clients.All.SendLogToClients(logEntry);
            }
        }

        public async Task SendLogDebugAsync(string serviceName, string message)
        {
            if (_appSettings.EnableDashboardAccess)
            {
                var logEntry = new LogEntryModel(LogType.Debug,
                                             serviceName,
                                             message);
                await _logHub.Clients.All.SendLogToClients(logEntry);
            }
        }

        public async Task SendLogErrorAsync(string serviceName, string message)
        {
            if (_appSettings.EnableDashboardAccess)
            {
                var logEntry = new LogEntryModel(LogType.Error,
                                             serviceName,
                                             message);
                await _logHub.Clients.All.SendLogToClients(logEntry);
            }
        }

    }
}
