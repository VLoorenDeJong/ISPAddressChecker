namespace ISPAddressChecker.Interfaces
{
    public interface ITimerService
    {
        void Dispose();
        TimeSpan GetUptime();
        void StartISPCheckTimers();
    }
}