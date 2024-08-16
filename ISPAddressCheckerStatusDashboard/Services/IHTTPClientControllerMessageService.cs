using ISPAddressChecker.Models;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public interface IHTTPClientControllerMessageService
    {
        string SelectedColour { get; }

        event Action OnChange;

        LogEntryModel SendLogMessageToDashboard(string color);
    }
}