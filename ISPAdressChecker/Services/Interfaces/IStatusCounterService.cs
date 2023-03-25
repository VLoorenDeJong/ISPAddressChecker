namespace ISPAdressChecker.Services.Interfaces
{
    public interface IStatusCounterService
    {
        void AddStartdateRequested();
        void AddStatusUpdateRequested();
        int GetStartdateRequested();
        int GetStatusUpdateRequested();
    }
}