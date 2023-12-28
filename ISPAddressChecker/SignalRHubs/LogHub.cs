using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Constants;
using ISPAddressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ISPAddressChecker.SignalRHubs
{
    public class LogHub : Hub<ILogHub>
    {
        private ILogger<LogHub> _logger;

        public LogHub(ILogger<LogHub> logger)
        {
            _logger = logger;
        }

        public async Task SendLogToClients(LogEntryModel logEntry)
        {
            _logger.LogInformation("ISPAddressCheckerAPI.SignalRHubs -> {method} -> called", LogHubMethods.SendLogToClients);
            await Clients.All.SendLogToClients(logEntry);
        }

    }    
}