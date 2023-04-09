using ISPAdressChecker.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using ISPAdressChecker.Options;
using Microsoft.AspNetCore.Mvc;
using ISPAdressChecker.Models;
using Microsoft.AspNetCore.Mvc;



namespace ISPAdressChecker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ISPAddressCheckerStatusController : ControllerBase
    {
        private readonly ITimerService _timerService;
        private readonly IISPAdressCounterService _ISPAddressCounterService;
        private readonly IStatusCounterService _statusCounterService;
        private readonly ILogger<ISPAddressCheckerStatusController> _logger;
        private readonly ApplicationSettingsOptions _applicationSettingsOptions;
        private readonly IISPAddressService _iSPAddressService;

        public ISPAddressCheckerStatusController(ITimerService timerService, 
                                                 IISPAdressCounterService ISPAddressCounterService, 
                                                 IStatusCounterService statusCounterService, 
                                                 IOptions<ApplicationSettingsOptions> applicationSettingsOptions, 
                                                 ILogger<ISPAddressCheckerStatusController> logger,
                                                 IISPAddressService iSPAddressService)
        {
            _timerService = timerService;
            _statusCounterService = statusCounterService;
            _ISPAddressCounterService = ISPAddressCounterService;
            _applicationSettingsOptions = applicationSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(applicationSettingsOptions));
            _iSPAddressService = iSPAddressService;
            _logger = logger;
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
                _logger.LogInformation("ISPAddressCheckIntervalInMinutes -> Dashboard not enabled (appsettings:EnableDashboardAccess)");
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
                _logger.LogInformation("ISPAddressCheckIntervalInMinutes -> Dashboard not enabled (appsettings:EnableDashboardAccess)");
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
                _logger.LogInformation("ISPAddressCheckIntervalInMinutes -> Dashboard not enabled (appsettings:EnableDashboardAccess)");
                return Forbid();
            }

            _statusCounterService.AddISPAddressCheckIntervalRequested();

            double output = _applicationSettingsOptions.TimeIntervalInMinutes;
            _logger.LogInformation("ISPAddressCheckIntervalInMinutes -> interval: {interval} (minutes)", _applicationSettingsOptions.TimeIntervalInMinutes);

            return Ok(output);
        }


        [HttpPost("ISPAddressCheckSendEmail", Name = "ISPAddressCheckSendEmail")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ActionReportModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ActionReportModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ActionReportModel), StatusCodes.Status500InternalServerError)]
        public ActionResult<ActionReportModel> ISPAddressCheckSendEmail(SendEmailModel emailRequest)
        {
            ActionReportModel report = new();

            if (!_applicationSettingsOptions.EnableDashboardAccess)
            {
                _logger.LogInformation("ISPAddressCheckSendEmail -> Dashboard not enabled (appsettings:EnableDashboardAccess)");
                return Forbid();
            }

            if (emailRequest is not null)
            {
                report.Id = emailRequest?.Id;


                _logger.LogInformation("ISPAddressCheckSendEmail -> RequestId: {id}, EmailAddressValidated:{valid}, emailAddress:{emailAddress}", report.Id, emailRequest!.EmailValidated, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress));
                if (emailRequest.EmailValidated)
                {
                    switch (emailRequest.EmailType)
                    {
                        case Models.Enums.SendEmailTypeEnum.HeartBeatEmail:

                            // ToDo make application send email with success ActionReportModel return
                            report.Success = true;

                            if (report.Success)
                            {
                                _statusCounterService.AddISPHeartbeatEmailRequested();
                                _logger.LogInformation("ISPAddressCheckSendEmailRequest -> RequestId: {id}, SendEmailTypeEnum.HeartBeatEmail -> E-mail address:{email}, success: {success}", report.Id, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), report.Success);

                                return Ok(report);
                            }
                            break;
                        case Models.Enums.SendEmailTypeEnum.ISPAddressChanged:

                            // ToDo make application send email with success ActionReportModel return
                            report.Success = true;

                            if (report.Success)
                            {
                                _statusCounterService.AddISPISPAddressChangedEmailRequested();
                                _logger.LogInformation("ISPAddressCheckSendEmailRequest -> RequestId: {id}, SendEmailTypeEnum.ISPAddressChanged -> E-mail address:{email}, success: {success}", report.Id, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), report.Success);

                                return Ok(report);
                            }
                            break;
                    }
                }
                if (!report.Success)
                {
                    _logger.LogInformation("ISPAddressCheckSendEmailRequest -> Failed -> RequestId: {id}, EmailType:{type} -> E-mail address:{email}, message: {message}", report.Id, emailRequest.EmailType, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), report.Message);
                    return BadRequest(report);
                }
            }

            _logger.LogInformation("ISPAddressCheckSendEmailRequest -> Failed -> RequestId: {id}, EmailType:{type} -> E-mail address:{email}, message: {message}", report.Id, emailRequest?.EmailType, Helpers.StringHelpers.MakeEmailAddressLogReady(emailRequest?.EmailAddress), report.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to send email", report });
        }

        [HttpGet("ISPAddressCheckAPIEndpointURL", Name = "ISPAddressCheckAPIEndpointURL")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("text/plain")]
        public ActionResult<string> ISPAddressCheckAPIEndpointURL()
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
    }
}
