namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface IISPAddressCheckerStatusService
    {
        ISPAddressCheckerStatusUpdateModel CurrentStatus { get; }

        event Action OnChange;

        Task GetCurrentISPCheckerStatus();
    }
}