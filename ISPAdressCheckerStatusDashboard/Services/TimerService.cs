using System.Diagnostics;
using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{

    public class TimerService : ITimerService
    {
        private readonly Timer timer;
        private IOpenAPIClient _apiClient;
        private readonly ILogger<TimerService> _logger;

        private TimeSpan APIUpTime { get; set; }
        public DateTimeOffset APIStartDateTime { get; private set; }
        public string UptimeString { get; private set; }

        public TimerService()
        {

        }
        public TimerService(IOpenAPIClient aPIClient, ILogger<TimerService> logger)
        {
            _apiClient = aPIClient;
            _logger = logger;
            timer = new Timer(state => HandleUptimeCalculation(), null, 0, 1000);
        }

        public async Task StartTimers()
        {
            APIStartDateTime = await GetApiUptimeAsync();
        }

        private async Task<DateTimeOffset> GetApiUptimeAsync()
        {
            DateTimeOffset output = new();
            try
            {
                _logger.LogInformation("GetApiUptimeAsync -> getting start date time");
                output = await _apiClient.GetStartDateTimeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetApiUptimeAsync -> Error fetching date time, message: {message}", ex.Message);
            }

            return output;
        }

        private void HandleUptimeCalculation()
        {
            var iets = DateTimeOffset.MinValue;
            if (APIStartDateTime != DateTimeOffset.MinValue)
            {
                APIUpTime = CalculateUptime(APIStartDateTime);
                UptimeString = string.Format("{0:%d} days, {0:%h}:{0:%m}:{0:%s}", APIUpTime);
            }

        }

        private TimeSpan CalculateUptime(DateTimeOffset startDateTime)
        {
            TimeSpan apiUpTime = DateTimeOffset.UtcNow - startDateTime;
            return apiUpTime;
        }
    }
}




