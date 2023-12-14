using ISPAddressChecker.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using ISPAddressChecker.Options;
using Microsoft.AspNetCore.Mvc;
using ISPAddressChecker.Models;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using static ISPAddressChecker.Models.Enums.Constants;
using ISPAddressChecker.Helpers;

namespace ISPAddressChecker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ISPAddressCheckerStatusController : ControllerBase
    {
        private readonly ITimerService _timerService;
        private readonly IISPAddressCounterService _ISPAddressCounterService;
        private readonly IStatusCounterService _statusCounterService;
        private readonly IISPAddressService _iSPAddressService;
        private readonly IEmailService _emailService;
        private readonly ApplicationSettingsOptions _applicationSettingsOptions;
        private readonly ILogger<ISPAddressCheckerStatusController> _logger;
        private readonly ILogHubService _loghub;
        private readonly IUrlHelper _urlHelper;
        private readonly string serviceName = nameof(ISPAddressCheckerStatusController);

        public ISPAddressCheckerStatusController(
            ITimerService timerService,
            IISPAddressCounterService ISPAddressCounterService,
            IStatusCounterService statusCounterService,
            IISPAddressService iSPAddressService,
            IEmailService emailService,
            IOptions<ApplicationSettingsOptions> applicationSettingsOptions,
            ILogger<ISPAddressCheckerStatusController> logger,
            ILogHubService loghub,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor
            )
        {
            _timerService = timerService;
            _ISPAddressCounterService = ISPAddressCounterService;
            _statusCounterService = statusCounterService;
            _iSPAddressService = iSPAddressService;
            _emailService = emailService;
            _applicationSettingsOptions = applicationSettingsOptions.Value ?? throw new ArgumentNullException(nameof(applicationSettingsOptions));
            _logger = logger;
            _loghub = loghub;

            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext!);
        }

        [HttpGet("status", Name = "GetStatusUpdate")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ISPAddressCheckerStatusUpdateModel), StatusCodes.Status200OK)]
        public ActionResult<ISPAddressCheckerStatusUpdateModel> GetStatusUpdate()
        {
            _logger.LogInformation("GetStatusUpdate -> Status update has been requested");

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogWarning("ISPAddressCheckIntervalInMinutes -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }
            _statusCounterService.AddStatusUpdateRequested();

            ISPAddressCheckerStatusUpdateModel output = new ISPAddressCheckerStatusUpdateModel(_ISPAddressCounterService, _statusCounterService, _timerService, _iSPAddressService);

            return Ok(output);
        }

        [HttpGet("StartDateTime", Name = "GetStartDateTime")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DateTimeOffset), StatusCodes.Status200OK)]
        public ActionResult<DateTimeOffset> GetStartDateTime()
        {
            _logger.LogInformation("GetStartDateTime -> Start date has been requested");

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogWarning("ISPAddressCheckIntervalInMinutes -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }

            _statusCounterService.AddStartdateRequested();
            DateTimeOffset output = _timerService.GetStartDateTime();

            return Ok(output);
        }

        [HttpGet("ISPAddressCheckIntervalInMinutes", Name = "ISPAddressCheckIntervalInMinutes")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
        public ActionResult<double> ISPAddressCheckIntervalInMinutes()
        {
            _logger.LogInformation("ISPAddressCheckIntervalInMinutes -> ISP address check interval has been requested (minutes)");

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogWarning("ISPAddressCheckIntervalInMinutes -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }

            _statusCounterService.AddISPAddressCheckIntervalRequested();

            double output = _applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes;
            _logger.LogInformation("ISPAddressCheckIntervalInMinutes -> interval: {interval} (minutes)", output);

            return Ok(output);
        }


        [HttpPost("ISPAddressCheckSendEmail", Name = "ISPAddressCheckSendEmail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ActionReportModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionReportModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ActionReportModel), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ActionReportModel>> ISPAddressCheckSendEmail(SendEmailModel emailRequest)
        {
            ActionReportModel report = new();

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogWarning("ISPAddressCheckSendEmail -> Dashboard not enabled (appsettings:EnableDashboardAccess)");
                await _loghub.SendLogWarningAsync(serviceName, "ISPAddressCheckSendEmail -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }

            if (emailRequest is not null)
            {
                _logger.LogInformation("ISPAddressCheckSendEmail -> RequestId: {id}, EmailAddressValidated:{valid}, emailAddress:{emailAddress}", emailRequest.Id, emailRequest!.EmailValidated, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress));
                await _loghub.SendLogInfoAsync(serviceName, $"RequestId: {emailRequest.Id}, ISPAddressCheckSendEmail -> EmailAddressValidated:{emailRequest!.EmailValidated}, emailAddress:{Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress)}");

                if (emailRequest.EmailValidated)
                {
                    switch (emailRequest.EmailType)
                    {
                        case Models.Enums.SendEmailTypeEnum.HeartBeatEmail:

                            Dictionary<string, string> mocValues = new Dictionary<string, string>();

                            foreach (string? externalAPI in _applicationSettingsOptions.BackupAPIS!)
                            {
                                mocValues.Add($"{externalAPI}", "ISP address return value will be put here");
                            }

                            report = await _emailService.SendHeartBeatEmail(_ISPAddressCounterService, _iSPAddressService.GetOldISPAddress(), _iSPAddressService.GetCurrentISPAddress(), _iSPAddressService.GetNewISPAddress(), mocValues, emailRequest);

                            if (report.Success)
                            {
                                _statusCounterService.AddISPHeartbeatEmailRequested();
                                _logger.LogInformation("ISPAddressCheckSendEmail -> RequestId: {id}, SendEmailTypeEnum: {enum} -> E-mail address:{email}, success: {success}", emailRequest.Id, report.SendEmailTypeEnum, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), report.Success);
                                await _loghub.SendLogInfoAsync(serviceName, $"RequestId: {emailRequest.Id}, ISPAddressCheckSendEmail -> SendEmailTypeEnum: {report.SendEmailTypeEnum.ToString()} -> E-mail address:{Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress)}, success: {report.Success}");

                                return Ok(report);
                            }
                            break;
                        case Models.Enums.SendEmailTypeEnum.ISPAddressChanged:

                            report = await _emailService.SendISPAddressChangedEmail(_iSPAddressService.GetExternalISPAddress(), _iSPAddressService.GetOldISPAddress(), _ISPAddressCounterService, _applicationSettingsOptions.ISPAddressCheckFrequencyInMinutes, emailRequest);

                            if (report.Success)
                            {
                                _statusCounterService.AddISPISPAddressChangedEmailRequested();
                                _logger.LogInformation("ISPAddressCheckSendEmail -> RequestId: {id}, SendEmailTypeEnum: {enum} -> E-mail address:{email}, success: {success}", emailRequest.Id, report.SendEmailTypeEnum, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), report.Success);
                                await _loghub.SendLogInfoAsync(serviceName, $" RequestId: {emailRequest.Id}, ISPAddressCheckSendEmail ->SendEmailTypeEnum: {report.SendEmailTypeEnum.ToString()} -> E-mail address:{Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress)}, success: {report.Success}");

                                return Ok(report);
                            }
                            break;
                    }
                }
                // ToDo: check this clause
                if (report.Success)
                {
                    _logger.LogWarning("ISPAddressCheckSendEmail -> Failed -> RequestId: {id}, EmailType:{type} -> E-mail address:{email}, message: {message}", emailRequest.Id, emailRequest.EmailType, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), report.Message);
                    await _loghub.SendLogWarningAsync(serviceName, $"RequestId: {emailRequest.Id}, ISPAddressCheckSendEmail -> Failed ->  EmailType:{emailRequest.EmailType}, E-mail address:{Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress)}, message: {report.Message}");

                    return BadRequest(report);
                }
            }

            _logger.LogInformation("ISPAddressCheckSendEmail -> Failed -> RequestId: {id}, EmailType:{type} -> E-mail address:{email}, message: {message}", emailRequest.Id, emailRequest?.EmailType, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest!.EmailAddress), report.Message);
            await _loghub.SendLogWarningAsync(serviceName, $"RequestId: {emailRequest.Id}, ISPAddressCheckSendEmail -> Failed ->  EmailType: {emailRequest?.EmailType}, E-mail address:{Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest!.EmailAddress)}, message: {report.Message}");

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to send email", report });
        }

        [HttpGet("ISPAddressCheckAPIWebEndpointURL", Name = "ISPAddressCheckAPIWebEndpointURL")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("text/plain")]
        public ActionResult<string> ISPAddressCheckAPIWebEndpointURL()
        {
            _logger.LogInformation("ISPAddressCheckAPIEndpoint -> ISP address check Endpoint url has been requested");

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogInformation("ISPAddressCheckAPIEndpoint -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }

            _statusCounterService.AddISPAddressCheckIntervalRequested();

            string output = string.Empty;

            output = _applicationSettingsOptions.APIEndpointURL!;
            _logger.LogInformation("ISPAddressCheckAPIEndpoint ->  Endpoint url: {url} ", _applicationSettingsOptions!.APIEndpointURL);

            return Ok(output);
        }

        [HttpGet("ISPAddressGetAPILoghubURL", Name = "ISPAddressGetAPILoghubURL")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("text/plain")]
        public async Task<ActionResult<string>> ISPAddressGetAPILoghubURL()
        {

            _logger.LogInformation("ISPAddressGetAPILoghubURL -> ISP address check LogHub URL has been requested");
            await _loghub.SendLogInfoAsync(serviceName, "ISPAddressGetAPILoghubURL -> ISP address check LogHub URL has been requested");

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogInformation("ISPAddressGetAPILoghubURL -> Dashboard not enabled (appsettings:EnableDashboardAccess)");
                await _loghub.SendLogWarningAsync(serviceName, "ISPAddressGetAPILoghubURL -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }


            var request = _urlHelper.ActionContext.HttpContext.Request;
            var host = request.Host.Value; // This includes the port if it's included in the request
            var protocol = request.Scheme;
            var baseUrl = $"{protocol}://{host}";
            string output = $"{baseUrl}{SignalRHubUrls.LogHubURL}";

            string dashboardOutput = $"{protocol}://{StringHelpers.MakeHttpRequestHostDashboardReady(host)}{SignalRHubUrls.LogHubURL}";

            if (!string.IsNullOrWhiteSpace(output))
            {
                _logger.LogInformation("ISPAddressGetAPILoghubURL ->  LogHub URL: {url} ", output);

                await _loghub.SendLogInfoAsync(serviceName, $"ISPAddressGetAPILoghubURL ->  LogHub URL: {dashboardOutput}");
                return Ok(output);
            }
            else
            {
                return BadRequest();
            }
        }
        [HttpGet("ISPAddressGetAPIClockhubURL", Name = "ISPAddressGetAPIClockhubURL")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("text/plain")]
        public ActionResult<string> ISPAddressGetAPIClockhubURL()
        {

            _logger.LogInformation("ISPAddressGetAPIClockhubURL -> ISP address check Clock Hub URL has been requested");

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogInformation("ISPAddressGetAPIClockhubURL -> Dashboard not enabled (appsettings:EnableDashboardAccess)");

                return Forbid();
            }

            var request = _urlHelper.ActionContext.HttpContext.Request;
            var host = request.Host.Value; // This includes the port if it's included in the request
            var protocol = request.Scheme;
            var baseUrl = $"{protocol}://{host}";
            string output = $"{baseUrl}{SignalRHubUrls.ClockHubURL}";

            if (!string.IsNullOrWhiteSpace(output))
            {
                _logger.LogInformation("ISPAddressGetAPIClockhubURL ->  ClockHub URL: {url} ", output);
                return Ok(output);
            }
            else
            {
                _logger.LogWarning("ISPAddressGetAPIClockhubURL -> Bad Request: ClockHub URL: {url} ", output);
                return BadRequest();
            }
        }

    }
}
