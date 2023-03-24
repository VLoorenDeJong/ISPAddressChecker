using ISPAddressChecker.Helpers;
using ISPAddressChecker.Interfaces;
using ISPAddressChecker.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ISPAddressChecker.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class HTTPController : ControllerBase
    {
        private readonly IISPAddressCounterService _counterService;
        private readonly ILogger<HTTPController> _logger;

        public HTTPController(ILogger<HTTPController> logger, IISPAddressCounterService counterService)
        {
            _counterService = counterService;
            _logger = logger;
        }

        [HttpGet("MyISPAddress", Name = "MyISPAddress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> GetIpAddress()
        {
            _logger.LogInformation("ISP address has been requested");
            _counterService.AddISPEndpointRequests();

            HttpContext context = HttpContext;
            string? outputString = string.Empty;

            outputString = HttpContext?.Connection?.RemoteIpAddress?.ToString();

            // check if it's an IPv6 address and convert to IPv4 format if necessary
            if (!string.IsNullOrWhiteSpace(outputString))
            {
                if (outputString.Contains(":"))
                {
                    outputString = HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
                }
            }

            // check for the X-Forwarded-For header to get the client IP address behind the proxy
            string? xForwardedForHeader = HttpContext?.Request?.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(xForwardedForHeader))
            {
                string[] addresses = xForwardedForHeader.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (addresses.Length > 0)
                {
                    outputString = addresses[0].Trim();
                }
            }

            if (!string.IsNullOrEmpty(outputString))
            {
                string logInfo = string.Empty;
                if (!string.IsNullOrWhiteSpace(outputString))
                {
                    logInfo = StringHelpers.MakeISPAddressLogReady(outputString);
                }
                else
                {
                    logInfo = outputString;
                }
                _logger.LogInformation("Success address returned:{logInfo}", logInfo);
                return outputString;
            }
            else
            {
                _logger.LogError("Something went wrong output was: {outputString}", outputString);
                return "What?!?";
            }

        }
    }
}
