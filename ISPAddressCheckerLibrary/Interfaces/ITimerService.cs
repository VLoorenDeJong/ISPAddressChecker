namespace ISPAddressChecker.Interfaces
{
    public interface ITimerService
    {
        void Dispose();
        DateTimeOffset GetStartDateTime();
        TimeSpan GetUptime();
        void StartISPCheckTimers();
    }
}