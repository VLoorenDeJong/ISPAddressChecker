using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Options;
using ISPAddressChecker.Helpers;
using ISPAddressChecker.Models;

namespace ISPAddressCheckerAPI.Services
{
    public class ApplicationService : IApplicationService, IHostedService
    {
        private readonly APIApplicationSettingsOptions _applicationSettingsOptions;
        private readonly APIEmailSettingsOptions _emailSettingsOptions;
        private readonly ITimerService _timerService;
        private readonly IAPIEmailService _emailService;
        private readonly IISPAddressCounterService _counterService;
        private readonly ICheckISPAddressService _checkISPAddressService;
        private readonly ILogger _logger;
        private readonly IAPIConfigCheckService _configCheckService;

        private bool configSuccess = false;

        public ApplicationService(ILogger<ApplicationService> logger
                                , IOptions<APIApplicationSettingsOptions> applicationSettingsOptions
                                , IOptions<APIEmailSettingsOptions> emailSettingsOptions
                                , ITimerService timerService
                                , IAPIEmailService emailService
                                , IISPAddressCounterService counterService
                                , ICheckISPAddressService checkISPAddressService
                                , IAPIConfigCheckService configCheckService)
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions!.Value!;
            _emailSettingsOptions = emailSettingsOptions!.Value;
            _timerService = timerService;
            _emailService = emailService;
            _counterService = counterService;
            _checkISPAddressService = checkISPAddressService;
            _configCheckService = configCheckService;

            configSuccess = CheckAppsettings();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            switch (configSuccess)
            {
                case true:
                    _logger.LogInformation("StartAsync -> FirstStart: configSuccess: {configSuccess}", configSuccess);
                    _timerService!.StartISPCheckTimers();
                    await _checkISPAddressService.HeartBeatCheck(_timerService.GetUptime());
                    break;
                case false:
                    _logger.LogInformation("StartAsync -> FirstStart: configSuccess: {configSuccess}", configSuccess);
                    await StopAsync(default);
                    break;
            }           
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => _timerService!.Dispose());
        }

        private bool CheckAppsettings()
        {
            _logger.LogInformation("CheckAppsettings -> Appsettings");
            bool appsettingsConfigSuccess = true;

            if (_applicationSettingsOptions is not null)
            {
                bool mailConfigured = true;
                ConfigErrorReportModel report = new();

                _logger.LogInformation("CheckAppsettings -> MandatoryConfigurationChecks STARTED, mail configured: {config}", mailConfigured);
                mailConfigured = _configCheckService.MandatoryConfigurationChecks(_emailSettingsOptions, _applicationSettingsOptions, _logger);

                _logger.LogInformation("CheckAppsettings -> mailConfigured: {mailConfigured}", mailConfigured);
                switch (mailConfigured)
                {
                    case false:
                        appsettingsConfigSuccess = false;
                        _logger.LogInformation("CheckAppsettings -> appsettingsConfigSuccess: {appsettingsConfigSuccess}", appsettingsConfigSuccess);
                        return appsettingsConfigSuccess;
                    case true:
                        _logger.LogInformation("CheckAppsettings -> appsettingsConfigSuccess: {appsettingsConfigSuccess}", appsettingsConfigSuccess);
                        report = _configCheckService.DefaultSettingsCheck(_emailSettingsOptions,_applicationSettingsOptions, _logger);
                        break;
                }

                switch (report.ChecksPassed)
                {
                    case false:
                        _emailService.SendConfigErrorMail(report.ErrorMessage!);
                        appsettingsConfigSuccess = false;
                        _logger.LogInformation("CheckAppsettings -> report.ChecksPassed: {passed}, Messages:{messages}", report.ChecksPassed.ToString(), report.ErrorMessage);
                        return appsettingsConfigSuccess;
                    case true:
                        _emailService.SendConfigSuccessMail(_counterService);
                        _logger.LogInformation("CheckAppsettings -> report.ChecksPassed: {passed}", report.ChecksPassed.ToString());
                        break;
                }
            }
            else
            {
                _logger.LogInformation("CheckAppsettings -> _emailSettings is null");
                throw new ArgumentException();
            }

            _logger.LogInformation("CheckAppsettings -> appsettingsConfigSuccess: {appsettingsConfigSuccess}", appsettingsConfigSuccess);
            return appsettingsConfigSuccess;
        }
    }    
}
