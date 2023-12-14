using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Enums;
using ISPAddressCheckerStatusDashboard.Services.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using static ISPAddressChecker.Models.Enums.Constants;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public class LogHubService : ILogHubService, IAsyncDisposable
    {
        public List<LogEntryModel> logEntries { get; private set; }

        public event Action OnChange;

        private IRequestISPAddressService _requestISPAddressService;
        private readonly ILogger<LogHubService> _logger;
        private HubConnection? logHubConnection;

        public LogHubService(IRequestISPAddressService requestISPAddressService
                            , ILogger<LogHubService> logger)
        {
            logEntries = new();
            _requestISPAddressService = requestISPAddressService;
            _logger = logger;
        }

        public async Task InstatiateLogHub()
        {
            string logHubUrl = string.Empty;
            int counter = 0;
            bool continueLooping = true;

            do
            {
                logHubUrl = await _requestISPAddressService.GetLogHubURLAsync();
                counter++;
                if (counter > 3 || !string.IsNullOrWhiteSpace(logHubUrl))
                {
                    continueLooping = false;
                }
            }
            while (continueLooping);

            if (!string.IsNullOrWhiteSpace(logHubUrl))
            {
                logHubConnection = new HubConnectionBuilder()
                .WithUrl(logHubUrl)
                .WithAutomaticReconnect()
                    .Build();

                logHubConnection.On<LogEntryModel>(LogHubMethods.SendLogToClients, (logEntry) =>
                {
                    AddLogMessage(logEntry);
                });

                try
                {
                    await logHubConnection.StartAsync();
                    AddLogMessage(new LogEntryModel
                    {
                        LogType = LogType.Information,
                        Message = "Hub connection established.",
                        Service = "LogHub",
                        Time = DateTime.UtcNow
                    });
                    _logger.LogInformation("Hub connection established.");
                }
                catch (Exception ex)
                {
                    AddLogMessage(new LogEntryModel
                    {
                        LogType = LogType.Error,
                        Message = $"Error establishing hub connection: {ex.Message}",
                        Service = "LogHub",
                        Time = DateTime.UtcNow
                    });
                    _logger.LogInformation($"Error establishing hub connection: {ex.Message}");
                }
            }
            else
            {
                AddLogMessage(new LogEntryModel
                {
                    LogType = LogType.Error,
                    Message = $"Error establishing hub connection, no URL fetched!",
                    Service = "LogMessageBoard",
                    Time = DateTime.UtcNow
                });
                AddLogMessage(new LogEntryModel
                {
                    LogType = LogType.Error,
                    Message = $"No URL fetched, is the API running?",
                    Service = "LogMessageBoard",
                    Time = DateTime.UtcNow
                });
                _logger.LogError($"Error establishing hub connection, no URL fetched!");
                _logger.LogWarning($"No URL fetched, is the API running?");
            }
        }

        public void AddLogMessage(LogEntryModel logEntry)
        {
            logEntries.Add(logEntry);
            logEntries = logEntries.OrderByDescending(x => x.Time).ToList();
            NotifyStateChanged();
        }

        public void NotifyStateChanged() => OnChange?.Invoke();

        public async ValueTask DisposeAsync()
        {
            if (logHubConnection is not null)
            {
                await logHubConnection.DisposeAsync();
            }
            logEntries.Clear();
        }
    }
}
