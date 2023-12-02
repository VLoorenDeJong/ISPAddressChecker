using ISPAddressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.VisualBasic;
using MyApplication;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public partial class ClockHubClient : IClock, IHostedService
    {
        private readonly ILogger<ClockHubClient> _logger;
        private HubConnection _connection;

        public ClockHubClient(ILogger<ClockHubClient> logger)
        {
            _logger = logger;

            _connection = new HubConnectionBuilder()
                .WithUrl(@"https://localhost:7235/hubs/clock")
                .Build();

            _connection.On<DateTime>(Strings.Events.TimeSent, ShowTime);
        }

        public Task ShowTime(DateTime currentTime)
        {
            _logger.LogInformation("{CurrentTime}", currentTime.ToShortTimeString());

            return Task.CompletedTask;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        { // Loop is here to wait until the server is running
            while (true)
            {
                try
                {
                    await _connection.StartAsync(cancellationToken);

                    break;
                }
                catch
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _connection.DisposeAsync();
        }
    }
    public static class Strings
    {
        public static class Events
        {
            public const string TimeSent = "TimeSent";
        }
    }
}
