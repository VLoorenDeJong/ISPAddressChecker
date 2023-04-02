using System.Diagnostics;
using ISPAdressCheckerStatusDashboard.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ISPAdressCheckerStatusDashboard.Services
{

    public class TimerService : ITimerService
    {
        private readonly IISPAddressCheckerStatusService _statusService;
        private IOpenAPIClient _apiClient;
        private readonly ILogger<TimerService> _logger;

        private Timer? ISPStatusUpdateTimer;
        private Timer? upTimeCalculatorTimer;


        private TimeSpan APIUpTime { get; set; }
        public DateTimeOffset APIStartDateTime { get; private set; }
        public string UptimeDays { get; private set; }
        public string UptimeClockString { get; private set; }

        private double minutesInterval = 61;

        public TimerService()
        {

        }

        public TimerService(IOpenAPIClient aPIClient, ILogger<TimerService> logger, IISPAddressCheckerStatusService statusService)
        {
            _apiClient = aPIClient;
            _logger = logger;
            _statusService = statusService;

            upTimeCalculatorTimer = new Timer(state => HandleUptimeCalculation(), null, 0, 1000);
        }

        public async Task StartTimers()
        {
            APIStartDateTime = await GetApiUptimeAsync();
            await StartStatusUpdateTimer();
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
                await _statusService.GetCurrentISPCheckerStatus();
            }, null, (int)(nextOccurrence - now).TotalMilliseconds + 5000, (int)statusUpdateInterval.TotalMilliseconds);
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




