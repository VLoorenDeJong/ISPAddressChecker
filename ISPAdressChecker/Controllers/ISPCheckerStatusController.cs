using ISPAdressChecker.Controllers;
using ISPAdressChecker.Interfaces;
using ISPAdressChecker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ISPAdressChecker.Controllers
{
    public class ISPCheckerStatusController : ControllerBase
    {
        private readonly ITimerService _timerService;
        private readonly IISPAdressCounterService _ISPAdressCounterService;
        private readonly IStatusCounterService _statusCounterService;
        private readonly ILogger<HTTPController> _logger;

        public ISPCheckerStatusController(ITimerService timerService, IISPAdressCounterService ISPAdressCounterService, IStatusCounterService statusCounterService, ILogger<HTTPController> logger)
        {
            _timerService = timerService;
            _ISPAdressCounterService = ISPAdressCounterService;
            _statusCounterService = statusCounterService;
            _logger = logger;
        }

        [HttpGet("APIStatusUpdate", Name = "APIStatusUpdate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(StatusUpdateModel), 200)]
        public ActionResult<StatusUpdateModel> GetStatusUpdate()
        {
            _logger.LogInformation("StatusUpdate has been requested");

            StatusUpdateModel output = new StatusUpdateModel(_ISPAdressCounterService, _statusCounterService, _timerService);

            return output;
        }
    }
}
