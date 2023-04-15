using ISPAdressChecker.Services.Interfaces;
using Microsoft.Extensions.Options;
using ISPAdressChecker.Helpers;
using ISPAdressChecker.Options;
using ISPAdressChecker.Models;
using System.Diagnostics;
using Microsoft.AspNet.SignalR.Hubs;

namespace ISPAdressChecker.Services
{
    public class TimerService : ITimerService
    {
        private readonly ApplicationSettingsOptions _applicationSettingsOptions;
        private readonly ICheckISPAddressService _ISPAdressService;
        private readonly IISPAdressCounterService _counterService;
        private readonly ILogger _logger;

        private DateTimeOffset StartDateTime = DateTimeOffset.UtcNow;
        private Timer? controlISPAdressCheckTimer;
        private Timer? ISPAdressCheckTimer;
        private Timer? HeartbeatemailTimer;

        private Stopwatch UpTime = new Stopwatch();

        private double ISPAdressCHeckInterval;

        public TimerService(ILogger<CheckISPAddressService> logger, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, IISPAdressCounterService counterService, ICheckISPAddressService ISPAdressService)
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;
            _counterService = counterService;
            _ISPAdressService = ISPAdressService;
        }

        public void StartISPCheckTimers()
        {
            _logger.LogInformation("StartISPCheckTimers -> start");
            ISPAdressCHeckInterval = (_applicationSettingsOptions.TimeIntervalInMinutes == 0) ? 60 : _applicationSettingsOptions.TimeIntervalInMinutes;

            _logger.LogInformation("ISPAdressCHeckInterval: {inter}(minutes), configured:{confInter}(minutes)", ISPAdressCHeckInterval, _applicationSettingsOptions.TimeIntervalInMinutes);


            ISPAdressCheckTimer = new Timer(async (state) => await _ISPAdressService.GetISPAddressAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(ISPAdressCHeckInterval));
            _logger.LogInformation("ISPAdressCheckTimer interval: {inter}(minutes), configured:{confInter}(minutes)", ISPAdressCHeckInterval, _applicationSettingsOptions.TimeIntervalInMinutes);

            controlISPAdressCheckTimer = new Timer(state => { _counterService.AddServiceCheckCounter(); }, null, TimeSpan.FromMinutes(ISPAdressCHeckInterval), TimeSpan.FromMinutes(ISPAdressCHeckInterval));
            _logger.LogInformation("ControlISPAdressCheckTimer interval: {inter}(minutes), configured:{confInter}(minutes)", ISPAdressCHeckInterval, _applicationSettingsOptions.TimeIntervalInMinutes);
            SetupHeartbeatTimer();
            UpTime.Start();
        }

        private void SetupHeartbeatTimer()
        {
            _logger.LogInformation("SetupHeartbeatTimer -> start");
            DateTime now = DateTime.Now;

            _logger.LogInformation("SetupHeartbeatTimer: {time}", DateTime.UtcNow);
            DateTime nextOccurrence = now.AddDays(((int)_applicationSettingsOptions.HeatbeatEmailDayOfWeek - (int)now.DayOfWeek + 7) % 7).Date.Add(_applicationSettingsOptions.HeatbeatEmailTimeOfDay);

            _logger.LogInformation("SetupHeartbeatTimer: nextOccurrence:{date}", nextOccurrence);
            if (nextOccurrence < now)
            {
                nextOccurrence = nextOccurrence.AddDays(_applicationSettingsOptions.HeatbeatEmailIntervalDays);
                _logger.LogInformation("SetupHeartbeatTimer: nextOccurrence + days:{date}", nextOccurrence);
            }

            TimeSpan heartBeatInterval = TimeSpan.FromDays(_applicationSettingsOptions.HeatbeatEmailIntervalDays);

            _logger.LogInformation("HeartBeatInterval: {heartBeatInterval}(Days)", heartBeatInterval);

            HeartbeatemailTimer = new Timer(async (state) =>
            {
                await _ISPAdressService.HeartBeatCheck();
            }, null, (int)(nextOccurrence - now).TotalMilliseconds, (int)heartBeatInterval.TotalMilliseconds);
        }

        public TimeSpan GetUptime()
        {
            _logger.LogInformation("GetUptime: {Elapsed}", UpTime.Elapsed);
            return UpTime.Elapsed;
        }

        public DateTimeOffset GetStartDateTime()
        {
            return StartDateTime;
        }

        public void Dispose()
        {
            _logger.LogInformation("Dispose: {time} ", DateTime.UtcNow);
            if (ISPAdressCheckTimer is not null) ISPAdressCheckTimer!.Dispose();
            if (controlISPAdressCheckTimer is not null) controlISPAdressCheckTimer!.Dispose();
            if (HeartbeatemailTimer is not null) HeartbeatemailTimer!.Dispose();
        }
    }
}
