namespace ISPAddressChecker.Interfaces
{
    public interface IStatusCounterService
    {
        void AddStatusUpdateRequested();
        int GetStatusUpdateRequested();
    }
}