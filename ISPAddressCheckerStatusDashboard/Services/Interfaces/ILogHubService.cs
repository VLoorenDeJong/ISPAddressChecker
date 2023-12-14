using ISPAddressChecker.Models;

namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface ILogHubService
    {
        List<LogEntryModel> logEntries { get; }

        event Action OnChange;

        void AddLogMessage(LogEntryModel logEntry);
        ValueTask DisposeAsync();
        Task InstatiateLogHub();
        void NotifyStateChanged();
    }
}