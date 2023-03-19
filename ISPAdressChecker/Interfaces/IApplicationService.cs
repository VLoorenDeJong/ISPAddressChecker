namespace ISPAdressChecker.Interfaces
{
    public interface IApplicationService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}