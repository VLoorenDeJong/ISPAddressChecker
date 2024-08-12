using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Helpers;
using ISPAddressChecker.Options;
using ISPAddressChecker.Models;
using System.Diagnostics;

namespace ISPAddressChecker.Services
{
    public class TimerService : ITimerService
    {
        private readonly APIApplicationSettingsOptions _applicationSettingsOptions;
        private readonly APIEmailSettingsOptions _emailSettingsOptions;
        private readonly ICheckISPAddressService _ISPAddressService;
        private readonly IISPAddressCounterService _counterService;
        private readonly ILogger _logger;

        private DateTimeOffset StartDateTime = DateTimeOffset.UtcNow;
        private Timer? controlISPAddressCheckTimer;
        private Timer? ISPAddressCheckTimer;
        private Timer? HeartbeatemailTimer;

        private Stopwatch UpTime = new Stopwatch();

        private double ISPAddressCHeckInterval;

        public TimerService(ILogger<TimerService> logger
                          , IOptions<APIApplicationSettingsOptions> applicationSettingsOptions
                          , IISPAddressCounterService counterService
                          , ICheckISPAddressService ISPAddressService
                          , IOptions<APIEmailSettingsOptions> emailSettingsOptions
                            )
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;

            _emailSettingsOptions = emailSettingsOptions!.Value;
            _counterService = counterService;
            _ISPAddressService = ISPAddressService;
        }

        public void StartISPCheckTimers()
        {
            _logger.LogInformation("StartISPCheckTimers -> start");
            ISPAddressCHeckInterval = (_applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes == 0) ? 60 : _applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes;

            _logger.LogInformation("ISPAddressCHeckInterval: {inter}(minutes), configured:{confInter}(minutes)", ISPAddressCHeckInterval, _applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes);


            ISPAddressCheckTimer = new Timer(async (state) => await _ISPAddressService.GetISPAddressAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(ISPAddressCHeckInterval));
            _logger.LogInformation("ISPAddressCheckTimer interval: {inter}(minutes), configured:{confInter}(minutes)", ISPAddressCHeckInterval, _applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes);

            controlISPAddressCheckTimer = new Timer(state => { _counterService.AddServiceCheckCounter(); }, null, TimeSpan.FromMinutes(ISPAddressCHeckInterval), TimeSpan.FromMinutes(ISPAddressCHeckInterval));
            _logger.LogInformation("ControlISPAddressCheckTimer interval: {inter}(minutes), configured:{confInter}(minutes)", ISPAddressCHeckInterval, _applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes);
            
            if(_emailSettingsOptions.HeartbeatEmailEnabled) SetupHeartbeatTimer();
            
            UpTime.Start();
        }

        private void SetupHeartbeatTimer()
        {
            _logger.LogInformation("SetupHeartbeatTimer -> start");
            DateTime now = DateTime.Now;

            _logger.LogInformation("SetupHeartbeatTimer: {time}", DateTime.UtcNow);
            DateTime nextOccurrence = now.AddDays(((int)_emailSettingsOptions.HeartbeatEmailDayOfWeek - (int)now.DayOfWeek + 7) % 7).Date.Add(_emailSettingsOptions.HeartbeatEmailTimeOfDay);

            _logger.LogInformation("SetupHeartbeatTimer: nextOccurrence:{date}", nextOccurrence);
            if (nextOccurrence < now)
            {
                nextOccurrence = nextOccurrence.AddDays(_emailSettingsOptions.HeartbeatEmailIntervalDays);
                _logger.LogInformation("SetupHeartbeatTimer: nextOccurrence + days:{date}", nextOccurrence);
            }

            TimeSpan heartBeatInterval = TimeSpan.FromDays(_emailSettingsOptions.HeartbeatEmailIntervalDays);

            _logger.LogInformation("HeartBeatInterval: {heartBeatInterval}(Days)", heartBeatInterval);

            HeartbeatemailTimer = new Timer(async (state) =>
            {
                await _ISPAddressService.HeartBeatCheck(GetUptime());
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
            if (ISPAddressCheckTimer is not null) ISPAddressCheckTimer!.Dispose();
            if (controlISPAddressCheckTimer is not null) controlISPAddressCheckTimer!.Dispose();
            if (HeartbeatemailTimer is not null) HeartbeatemailTimer!.Dispose();
        }
    }
}
