using ISPAddressChecker.Interfaces;
using ISPAddressChecker.Options;
using Microsoft.Extensions.Options;
using ISPAddressCheckerStatusDashboard;
using ISPAddressCheckerStatusDashboard.Services;

namespace ISPAddressCheckerDashboard.Services
{

    public class TimerService : IDashboardTimerService
    {
        private readonly IStatusService _statusService;
        private readonly ICounterService _counterService;
        private IOpenAPIClient _apiClient;
        private readonly ILogger<TimerService> _logger;

        private Timer? ISPStatusUpdateTimer;
        private Timer? upTimeCalculatorTimer;
        private readonly DashboardApplicationSettingsOptions _appSettings;

        private TimeSpan APIUpTime { get; set; }
        public DateTimeOffset APIStartDateTime { get; private set; }
        public string? UptimeDays { get; private set; }
        public string? UptimeClockString { get; private set; }

        private double minutesInterval = 61;

        bool timersStarted = false;

        public TimerService(IOpenAPIClient aPIClient, ILogger<TimerService> logger, IStatusService statusService, ICounterService counterservice, IOptions<DashboardApplicationSettingsOptions> appsettings)
        {
            _apiClient = aPIClient;
            _logger = logger;
            _statusService = statusService;
            _counterService = counterservice;
            upTimeCalculatorTimer = new Timer(state => HandleUptimeCalculation(), null, 0, 1000);
            _appSettings = appsettings.Value;
        }

        public async Task StartTimers()
        {
            timersStarted = true;
            APIStartDateTime = await GetApiUptimeAsync();
            await StartStatusUpdateTimer();
            StartEmailCounterResetTimer();
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
                _logger.LogError("GetApiUptimeAsync -> Error fetching start date time, message: {message}", ex.Message);
            }

            _logger.LogInformation("GetApiUptimeAsync -> start date time: {time}", output);

            return output;
        }

        private async Task StartStatusUpdateTimer()
        {
            try
            {
                _logger.LogInformation("StartStatusUpdateTimer -> API call -> ISPAddressCheckIntervalInMinutesAsync ");
                minutesInterval = await _apiClient.ISPAddressCheckIntervalInMinutesAsync();
                _logger.LogInformation("StartStatusUpdateTimer -> fetched interval: {int}", minutesInterval);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("StartStatusUpdateTimer -> ISPAddressCheckIntervalInMinutesAsync -> Exception message: {mess}", ex.Message);
            }

            _logger.LogInformation("StartStatusUpdateTimer -> start");
            DateTime now = DateTime.Now;

            _logger.LogInformation("StartStatusUpdateTimer -> dateTime.now: {time}", DateTime.UtcNow);
            DateTime nextOccurrence = now.AddMinutes(minutesInterval);

            _logger.LogInformation("StartStatusUpdateTimer -> nextOccurrence:{date}", nextOccurrence);
            if (nextOccurrence < now)
            {
                nextOccurrence = nextOccurrence.AddMinutes(minutesInterval);
                _logger.LogInformation("StartStatusUpdateTimer -> nextOccurrence {date}", nextOccurrence);
            }

            TimeSpan statusUpdateInterval = TimeSpan.FromMinutes(minutesInterval);

            _logger.LogInformation("StartStatusUpdateTimer -> {interval}(minutes)", statusUpdateInterval);

            ISPStatusUpdateTimer = new Timer(async (state) =>
            {
                await _statusService.GetStatus();
            }, null, (int)(nextOccurrence - now).TotalMilliseconds + 5000, (int)statusUpdateInterval.TotalMilliseconds);
        }

        private void StartEmailCounterResetTimer()
        {
            // Set the start time to 12 o'clock today
            DateTime startTime = DateTime.Today.AddHours(12);

            // Calculate the time interval until the next occurrence of the start time
            TimeSpan timeUntilNextOccurrence = startTime > DateTime.Now ? startTime - DateTime.Now : startTime.AddDays(1) - DateTime.Now;

            // Create a timer that triggers the method after 24 hours
            Timer timer = new Timer(async (state) =>
            {
                await Task.Run(() => _counterService.ResetEmailCounters());
            }, null, timeUntilNextOccurrence, TimeSpan.FromHours(_appSettings.EmailCounterResetTimeInHours));

            _logger.LogInformation("StartEmailCounterResetTimer -> {interval}(minutes)", timeUntilNextOccurrence);
            _logger.LogInformation("StartEmailCounterResetTimer -> timer set for {hours}(hours)", _appSettings.EmailCounterResetTimeInHours);

            // Testing code
            // TimeSpan timeUntilNextOccurrence =TimeSpan.FromSeconds(30);
            //Timer timer = new Timer(async (state) =>
            //{
            //    _counterService.ResetCounters();
            //}, null, timeUntilNextOccurrence, TimeSpan.FromSeconds(10));
        }


        public void ClearStatusUpdateTimer()
        {
            ISPStatusUpdateTimer = null;

            _logger.LogInformation("ClearStatusUpdateTimer -> timer cleared");
        }

        private void HandleUptimeCalculation()
        {
            if (APIStartDateTime != DateTimeOffset.MinValue)
            {
                APIUpTime = CalculateUptime(APIStartDateTime);
                UptimeDays = string.Format("{0:%d} days", APIUpTime);
                UptimeClockString = string.Format("{0:hh\\:mm\\:ss}", APIUpTime);
            }
           // _logger.LogInformation("HandleUptimeCalculation -> APIUpTime: {time}", APIUpTime);
        }

        private TimeSpan CalculateUptime(DateTimeOffset startDateTime)
        {
            TimeSpan apiUpTime = DateTimeOffset.UtcNow - startDateTime;
            return apiUpTime;
        }
    }
}




