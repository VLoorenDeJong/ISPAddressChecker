using ISPAdressChecker.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using ISPAdressChecker.Options;
using Microsoft.AspNetCore.Mvc;
using ISPAdressChecker.Models;


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

        public ISPAddressCheckerStatusController(ITimerService timerService, IISPAdressCounterService ISPAddressCounterService, IStatusCounterService statusCounterService, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, ILogger<ISPAddressCheckerStatusController> logger)
        {                                                                             
            _timerService = timerService;
            _statusCounterService = statusCounterService;
            _ISPAddressCounterService = ISPAddressCounterService;
            _applicationSettingsOptions = applicationSettingsOptions?.Value ?? throw new ArgumentNullException(nameof(applicationSettingsOptions));
            _logger = logger;
        }

        [HttpGet("status", Name = "GetStatusUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ISPAddressCheckerStatusUpdateModel), StatusCodes.Status200OK)]
        public ActionResult<ISPAddressCheckerStatusUpdateModel> GetStatusUpdate()
        {
            _statusCounterService.AddStatusUpdateRequested();
            _logger.LogInformation("Status update has been requested");

            if (!_applicationSettingsOptions.EnableStatusAccess)
            {
                return Forbid();
            }

            ISPAddressCheckerStatusUpdateModel output = new ISPAddressCheckerStatusUpdateModel(_ISPAddressCounterService, _statusCounterService, _timerService);

            return Ok(output);
        }

        [HttpGet("StartDateTime", Name = "GetStartDateTime")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DateTimeOffset), StatusCodes.Status200OK)]
        public ActionResult<DateTimeOffset> GetStartDateTime()
        {
            _statusCounterService.AddStartdateRequested();
            _logger.LogInformation("Start date has been requested");

            if (!_applicationSettingsOptions.EnableStatusAccess)
            {
                return Forbid();
            }

            DateTimeOffset output = _timerService.GetStartDateTime();

            return Ok(output);
        }       
    }
}
