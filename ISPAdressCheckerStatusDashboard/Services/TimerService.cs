using System.Diagnostics;
using ISPAdressCheckerStatusDashboard.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ISPAdressCheckerStatusDashboard.Services
{

    public class TimerService : ITimerService
    {
        private readonly IStatusService _statusService;
        private readonly IApplicationService _applicationService;
        private readonly ICounterService _counterService;
        private IOpenAPIClient _apiClient;
        private readonly ILogger<TimerService> _logger;

        private Timer? ISPStatusUpdateTimer;
        private Timer? upTimeCalculatorTimer;


        private TimeSpan APIUpTime { get; set; }
        public DateTimeOffset APIStartDateTime { get; private set; }
        public string UptimeDays { get; private set; }
        public string UptimeClockString { get; private set; }

        private double minutesInterval = 61;
        bool timersStarted = false;

        public TimerService()
        {

        }

        public TimerService(IOpenAPIClient aPIClient, ILogger<TimerService> logger, IStatusService statusService, IApplicationService applicationService, ICounterService counterservice)
        {
            _apiClient = aPIClient;
            _logger = logger;
            _statusService = statusService;
            _applicationService = applicationService;
            _counterService = counterservice;
            upTimeCalculatorTimer = new Timer(state => HandleUptimeCalculation(), null, 0, 1000);
        }

        public async Task StartTimers()
        {
            timersStarted = true;
            APIStartDateTime = await GetApiUptimeAsync();
            await StartStatusUpdateTimer();
            StartCounterResetTimer();
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

        private void StartCounterResetTimer()
        {
            // Set the start time to 12 o'clock today
            DateTime startTime = DateTime.Today.AddHours(12);

            // Calculate the time interval until the next occurrence of the start time
            TimeSpan timeUntilNextOccurrence = startTime > DateTime.Now ? startTime - DateTime.Now : startTime.AddDays(1) - DateTime.Now;

            // Create a timer that triggers the method after 24 hours
            Timer timer = new Timer(async (state) =>
            {
                _counterService.ResetCounters();
            }, null, timeUntilNextOccurrence, TimeSpan.FromHours(24));

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
        }

        private void HandleUptimeCalculation()
        {
            if (APIStartDateTime != DateTimeOffset.MinValue)
            {
                APIUpTime = CalculateUptime(APIStartDateTime);
                UptimeDays = string.Format("{0:%d} days", APIUpTime);
                UptimeClockString = string.Format("{0:hh\\:mm\\:ss}", APIUpTime);
            }
        }

        private TimeSpan CalculateUptime(DateTimeOffset startDateTime)
        {
            TimeSpan apiUpTime = DateTimeOffset.UtcNow - startDateTime;
            return apiUpTime;
        }
    }
}




