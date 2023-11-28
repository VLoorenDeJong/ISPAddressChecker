using ISPAdressChecker.Services.Interfaces;
using Microsoft.Extensions.Options;
using ISPAdressChecker.Options;
using ISPAdressChecker.Helpers;
using ISPAdressChecker.Models;

namespace ISPAdressChecker.Services
{
    public class ApplicationService : IApplicationService, IHostedService
    {
        private readonly ApplicationSettingsOptions _applicationSettingsOptions;
        private readonly ITimerService _timerService;
        private readonly IEmailService _emailService;
        private readonly IISPAdressCounterService _counterService;
        private readonly ICheckISPAddressService _checkISPAddressService;
        private readonly ILogger _logger;

        private bool configSuccess = false;

        public ApplicationService(ILogger<CheckISPAddressService> logger, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, ITimerService timerService, IEmailService emailService, IISPAdressCounterService counterService, ICheckISPAddressService checkISPAddressService)
        {
            _logger = logger;
            _applicationSettingsOptions = applicationSettingsOptions?.Value!;
            _timerService = timerService;
            _emailService = emailService;
            _counterService = counterService;
            _checkISPAddressService = checkISPAddressService; 
            configSuccess = CheckAppsettings();
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            switch (configSuccess)
            {
                case true:
                    _logger.LogInformation("StartAsync -> FirstStart: configSuccess: {configSuccess}", configSuccess);
                    _timerService!.StartISPCheckTimers();
                    await _checkISPAddressService.HeartBeatCheck();
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
                mailConfigured = ConfigHelpers.MandatoryConfigurationChecks(_applicationSettingsOptions, _logger);

                _logger.LogInformation("CheckAppsettings -> mailConfigured: {mailConfigured}", mailConfigured);
                switch (mailConfigured)
                {
                    case false:
                        appsettingsConfigSuccess = false;
                        _logger.LogInformation("CheckAppsettings -> appsettingsConfigSuccess: {appsettingsConfigSuccess}", appsettingsConfigSuccess);
                        return appsettingsConfigSuccess;
                    case true:
                        _logger.LogInformation("CheckAppsettings -> appsettingsConfigSuccess: {appsettingsConfigSuccess}", appsettingsConfigSuccess);
                        report = ConfigHelpers.DefaultSettingsCheck(_applicationSettingsOptions, _logger);
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
                _logger.LogInformation("CheckAppsettings -> _applicationSettingsOptions is null");
                throw new ArgumentException();
            }

            _logger.LogInformation("CheckAppsettings -> appsettingsConfigSuccess: {appsettingsConfigSuccess}", appsettingsConfigSuccess);
            return appsettingsConfigSuccess;
        }
    }    
}
