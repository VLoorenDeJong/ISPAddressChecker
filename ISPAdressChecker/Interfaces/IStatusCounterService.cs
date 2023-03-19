namespace ISPAdressChecker.Interfaces
{
    public interface IStatusCounterService
    {
        void AddStatusUpdateRequested();
        int GetStatusUpdateRequested();
    }
}