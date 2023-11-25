using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace MyApplication
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

    public interface IClock
    {
        Task ShowTime(DateTime currentTime);
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<ClockHub, IClock> _clockHub;

        public Worker(ILogger<Worker> logger, IHubContext<ClockHub, IClock> clockHub)
        {
            _logger = logger;
            _clockHub = clockHub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Time}", DateTime.Now);
                await _clockHub.Clients.All.ShowTime(DateTime.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

