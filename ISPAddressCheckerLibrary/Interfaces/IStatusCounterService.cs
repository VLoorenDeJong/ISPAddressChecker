namespace ISPAddressChecker.Interfaces
{
    public interface IStatusCounterService
    {
        void AddISPAddressCheckIntervalRequested();
        void AddISPHeartbeatEmailRequested();
        void AddISPISPAddressChangedEmailRequested();
        void AddStartdateRequested();
        void AddStatusUpdateRequested();
        int GetISPAddressCheckIntervalRequested();
        int GetISPHeartbeatEmailRequested();
        int GetISPISPAddressChangedEmailRequested();
        int GetStartdateRequested();
        int GetStatusUpdateRequested();
    }
}