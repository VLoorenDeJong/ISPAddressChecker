namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface ICounterService
    {
        void AddDashboardISPAddressRequestCounter();
        int GetDashboardISPAddressRequestCounter();
        int GetHeartbeatEmailRequestCounter();
        int GetISPAddressChangedRequestCounter();
        void ResetEmailCounters();
        void SubtractHeartbeatEmailRequestCounter();
        void SubtractISPAddressChangedRequestCounter();
    }
}