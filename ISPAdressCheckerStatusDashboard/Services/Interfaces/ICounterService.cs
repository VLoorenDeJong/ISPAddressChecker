namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface ICounterService
    {
        int GetHeartbeatEmailRequestCounter();
        int GetISPAddressChangedRequestCounter();
        void ResetCounters();
        void SubtractHeartbeatEmailRequestCounter();
        void SubtractISPAddressChangedRequestCounter();
    }
}