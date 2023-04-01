namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface ITimerService
    {
        DateTimeOffset APIStartDateTime { get; }
        string UptimeClockString { get; }
        string UptimeDays { get; }

        Task StartTimers();
    }
}