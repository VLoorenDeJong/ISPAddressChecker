using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Enums;
using ISPAddressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using static ISPAddressChecker.Models.Enums.Constants;

namespace ISPAddressChecker.SignalRHubs
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
            _logger.LogInformation("ISPAddressCheckerAPI.SignalRHubs -> {method} -> called", LogHubMethods.SendLogToClients);
            await Clients.All.SendLogToClients(logEntry);
        }

    }    
}