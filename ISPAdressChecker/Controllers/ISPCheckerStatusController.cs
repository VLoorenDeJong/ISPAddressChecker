using ISPAdressChecker.Controllers;
using ISPAdressChecker.Interfaces;
using ISPAdressChecker.Models;
using ISPAdressChecker.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ISPAdressChecker.Controllers
{
    public class ISPCheckerStatusController : ControllerBase
    {
        private readonly ITimerService _timerService;
        private readonly IISPAdressCounterService _ISPAdressCounterService;
        private readonly IStatusCounterService _statusCounterService;
        private readonly ILogger<HTTPController> _logger;
        private readonly ApplicationSettingsOptions _applicationSettingsOptions;

        public ISPCheckerStatusController(ITimerService timerService, IISPAdressCounterService ISPAdressCounterService, IStatusCounterService statusCounterService, IOptions<ApplicationSettingsOptions> applicationSettingsOptions, ILogger<HTTPController> logger)
        {
            _timerService = timerService;
            _statusCounterService = statusCounterService;
            _ISPAdressCounterService = ISPAdressCounterService;
            _applicationSettingsOptions = applicationSettingsOptions!.Value;
            _logger = logger;
        }

        [HttpGet("APIStatusUpdate", Name = "APIStatusUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(StatusUpdateModel), 200)]
        public ActionResult<StatusUpdateModel> GetStatusUpdate()
        {
            _statusCounterService.AddStatusUpdateRequested();
            _logger.LogInformation("StatusUpdate has been requested");


            StatusUpdateModel output = new StatusUpdateModel(_ISPAdressCounterService, _statusCounterService, _timerService);

            if (_applicationSettingsOptions.EnableStatusAccess) 
            {
                return Ok(output);
            }
            else
            {
                return Forbid();
            }
        }
    }
}
