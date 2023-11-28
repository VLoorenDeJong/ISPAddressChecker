using ISPAdressChecker.Models;

namespace ISPAdressChecker.SignalRHubs.Interfaces
{
    public interface ILogHub
    {
        Task SendLogToClients(LogEntryModel logEntry);
    }
}