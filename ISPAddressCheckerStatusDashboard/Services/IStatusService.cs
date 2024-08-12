using ISPAddressCheckerStatusDashboard;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public interface IStatusService
    {
        ISPAddressCheckerStatusUpdateModel CurrentStatus { get; }

        event Action OnChange;

        Task GetStatus();
    }
}