namespace ISPAddressChecker.Services.Interfaces
{
    public interface ITimerService
    {
        void Dispose();
        DateTimeOffset GetStartDateTime();
        TimeSpan GetUptime();
        void StartISPCheckTimers();
    }
}