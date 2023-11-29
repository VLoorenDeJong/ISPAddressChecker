using ISPAdressChecker.SignalRHubs.Interfaces;
using ISPAdressChecker.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using ISPAdressChecker.Models;
using ISPAdressChecker.Models.Enums;
using ISPAdressChecker.Services.Interfaces;

namespace ISPAdressChecker.Services
{
    public class LogHubService : ILogHubService
    {

        private readonly IHubContext<LogHub, ILogHub> _logHub;
        public LogHubService(IHubContext<LogHub, ILogHub> logHub)
        {
            _logHub = logHub;
        }

        public async Task SendLogInfoAsync(string serviceName, string message)
        {

            var logEntry = new LogEntryModel(LogType.Information,
                                             serviceName,
                                             message);
            await _logHub.Clients.All.SendLogToClients(logEntry);
        }

        public async Task SendLogWarningAsync(string serviceName, string message)
        {

            var logEntry = new LogEntryModel(LogType.Warning,
                                            serviceName,
                                            message);
            await _logHub.Clients.All.SendLogToClients(logEntry);
        }

        public async Task SendLogDebugAsync(string serviceName, string message)
        {

            var logEntry = new LogEntryModel(LogType.Debug,
                                             serviceName,
                                             message);
            await _logHub.Clients.All.SendLogToClients(logEntry);
        }

        public async Task SendLogErrorAsync(string serviceName, string message)
        {

            var logEntry = new LogEntryModel(LogType.Error,
                                             serviceName,
                                             message);
            await _logHub.Clients.All.SendLogToClients(logEntry);
        }

    }
}
