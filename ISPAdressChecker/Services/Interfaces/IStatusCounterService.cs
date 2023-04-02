namespace ISPAdressChecker.Services.Interfaces
{
    public interface IStatusCounterService
    {
        void AddISPAddressCheckIntervalRequested();
        void AddStartdateRequested();
        void AddStatusUpdateRequested();
        int GetISPAddressCheckIntervalRequested();
        int GetStartdateRequested();
        int GetStatusUpdateRequested();
    }
}