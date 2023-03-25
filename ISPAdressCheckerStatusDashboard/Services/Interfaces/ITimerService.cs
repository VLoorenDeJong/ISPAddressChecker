namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface ITimerService
    {
        DateTimeOffset APIStartDateTime { get; }
        public string UptimeString { get; }

        Task StartTimers();
    }
}