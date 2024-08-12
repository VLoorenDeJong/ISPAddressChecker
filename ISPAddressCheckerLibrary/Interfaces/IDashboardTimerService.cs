namespace ISPAddressChecker.Interfaces
{
    public interface IDashboardTimerService
    {
        DateTimeOffset APIStartDateTime { get; }
        string? UptimeClockString { get; }
        string? UptimeDays { get; }

        void ClearStatusUpdateTimer();
        Task StartTimers();
    }
}