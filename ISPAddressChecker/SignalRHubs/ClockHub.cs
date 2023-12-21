using ISPAddressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ISPAddressChecker.SignalRHubs
{
    public class ClockHub : Hub<IClock>
    {
        private ILogger<ClockHub> _logger;

        public ClockHub(ILogger<ClockHub> logger)
        {
            _logger = logger;
        }

        public async Task SendTimeToClients(DateTime dateTime)
        {
            _logger.LogInformation("SendTimeToClients called");
            await Clients.All.ShowTime(dateTime);
        }
    }

    public class ClockWorker : BackgroundService
    {
        private readonly ILogger<ClockWorker> _logger;
        private readonly IHubContext<ClockHub, IClock> _clockHub;

        public ClockWorker(ILogger<ClockWorker> logger, IHubContext<ClockHub, IClock> clockHub)
        {
            _logger = logger;
            _clockHub = clockHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("LogWorker running at: {Time}", DateTime.Now);
                await _clockHub.Clients.All.ShowTime(DateTime.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
