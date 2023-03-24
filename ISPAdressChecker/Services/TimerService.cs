using Microsoft.Extensions.Options;
using ISPAddressChecker.Interfaces;
using ISPAddressChecker.Helpers;
using ISPAddressChecker.Options;
using ISPAddressChecker.Models;
using System.Diagnostics;

namespace ISPAddressChecker.Services
{
    public class TimerService : ITimerService
    {
        private readonly ApplicationSettingsOptions _applicationSettingsOptions;
        private readonly ICheckISPAddressService _ISPAddressService;
        private readonly IISPAddressCounterService _counterService;
        private readonly ILogger _logger;

        private Timer? controlISPAddressCheckTimer;
        private Timer? ISPAddressCheckTimer;
        private Timer? HeartbeatemailTimer;

        private Stopwatch UpTime = new Stopwatch();

        private double ISPAddressCHeckInterval;

        public TimerService(ILogger<CheckISPAddressService> logger, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, IISPAddressCounterService counterService, ICheckISPAddressService ISPAddressService)
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;
            _counterService = counterService;
            _ISPAddressService = ISPAddressService;
        }

        public void StartISPCheckTimers()
        {
            _logger.LogInformation("StartISPCheckTimers -> start");
            ISPAddressCHeckInterval = (_applicationSettingsOptions.TimeIntervalInMinutes == 0) ? 60 : _applicationSettingsOptions.TimeIntervalInMinutes;

            _logger.LogInformation("ISPAddressCHeckInterval: {inter}(minutes), configured:{confInter}(minutes)", ISPAddressCHeckInterval, _applicationSettingsOptions.TimeIntervalInMinutes);


            ISPAddressCheckTimer = new Timer(async (state) => await _ISPAddressService.GetISPAddressAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(ISPAddressCHeckInterval));
            _logger.LogInformation("ISPAddressCheckTimer interval: {inter}(minutes), configured:{confInter}(minutes)", ISPAddressCHeckInterval, _applicationSettingsOptions.TimeIntervalInMinutes);

            controlISPAddressCheckTimer = new Timer(state => { _counterService.AddServiceCheckCounter(); }, null, TimeSpan.FromMinutes(ISPAddressCHeckInterval), TimeSpan.FromMinutes(ISPAddressCHeckInterval));
            _logger.LogInformation("ControlISPAddressCheckTimer interval: {inter}(minutes), configured:{confInter}(minutes)", ISPAddressCHeckInterval, _applicationSettingsOptions.TimeIntervalInMinutes);
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
                await _ISPAddressService.HeartBeatCheck();
            }, null, (int)(nextOccurrence - now).TotalMilliseconds, (int)heartBeatInterval.TotalMilliseconds);
        }

        public TimeSpan GetUptime()
        {
            _logger.LogInformation("GetUptime: {Elapsed}", UpTime.Elapsed);
            return UpTime.Elapsed;
        }

        public void Dispose()
        {
            _logger.LogInformation("Dispose: {time} ", DateTime.UtcNow);
            if (ISPAddressCheckTimer is not null) ISPAddressCheckTimer!.Dispose();
            if (controlISPAddressCheckTimer is not null) controlISPAddressCheckTimer!.Dispose();
            if (HeartbeatemailTimer is not null) HeartbeatemailTimer!.Dispose();
        }
    }
}
