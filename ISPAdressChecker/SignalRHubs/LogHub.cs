using ISPAdressChecker.Models;
using ISPAdressChecker.Models.Enums;
using ISPAdressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using static ISPAdressChecker.Models.Enums.Constants;

namespace ISPAdressChecker.SignalRHubs
{
    public class LogHub : Hub<ILogHub>
    {
        private ILogger<LogHub> _logger;

        public LogHub(ILogger<LogHub> logger)
        {
            _logger = logger;
        }

        // Todo Check if this is needed
        public async Task SendLogToClients(LogEntryModel logEntry)
        {
            _logger.LogInformation("ISPAdressChecker.SignalRHubs -> {method} -> called", LogHubMethods.SendLogToClients);
            await Clients.All.SendLogToClients(logEntry);
        }

    }    
}