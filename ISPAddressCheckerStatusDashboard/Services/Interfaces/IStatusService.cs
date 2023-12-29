using ISPAddressCheckerStatusDashboard;

namespace ISPAddressChecker.Interfaces
{
    public interface IStatusService
    {
        ISPAddressCheckerStatusUpdateModel CurrentStatus { get; }

        event Action OnChange;

        Task GetStatus();
    }
}