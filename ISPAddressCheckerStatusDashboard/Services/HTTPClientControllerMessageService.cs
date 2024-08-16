using ISPAddressChecker.Helpers;
using ISPAddressChecker.Models.Constants;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public class HTTPClientControllerMessageService : IHTTPClientControllerMessageService
    {
        public string SelectedColour { get; private set; }

        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();


        public ISPAddressChecker.Models.LogEntryModel SendLogMessageToDashboard(string color)
        {
            SelectedColour = color;

            ISPAddressChecker.Models.LogEntryModel newLogEntry = new();
            newLogEntry.LogType = LogType.Information;
            newLogEntry.Service = $"Dashboard -> RequestEmail";
            newLogEntry.Message = $"RequestId: something";

            NotifyStateChanged();

            return newLogEntry;
        }

    }





}

