namespace ISPAddressChecker.Interfaces
{
    public interface ILogHubService
    {
        Task SendLogDebugAsync(string serviceName, string message);
        Task SendLogErrorAsync(string serviceName, string message);
        Task SendLogInfoAsync(string serviceName, string message);
        Task SendLogWarningAsync(string serviceName, string message);
    }
}