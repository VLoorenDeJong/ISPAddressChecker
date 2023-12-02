using ISPAddressChecker.Models;

namespace ISPAddressChecker.SignalRHubs.Interfaces
{
    public interface ILogHub
    {
        Task SendLogToClients(LogEntryModel logEntry);
    }
}