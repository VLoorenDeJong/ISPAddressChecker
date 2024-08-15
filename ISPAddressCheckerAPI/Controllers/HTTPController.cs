using ISPAddressChecker.Helpers;
using ISPAddressChecker.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ISPAddressChecker.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class HTTPController : ControllerBase
    {
        private readonly IISPAddressCounterService _counterService;
        private readonly ILogger<HTTPController> _logger;
        private readonly IISPAddressService _iSPAddressService;
        private readonly ILogHubService _loghub;

        private readonly string serviceName = nameof(HTTPController);


        public HTTPController(ILogger<HTTPController> logger, IISPAddressCounterService counterService, IISPAddressService iSPAddressService, ILogHubService loghub)
        {
            _counterService = counterService;
            _logger = logger;
            _iSPAddressService = iSPAddressService;
            _loghub = loghub;

        }

        [HttpGet("MyISPAddress", Name = "MyISPAddress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetISPAddress()
        {
            _logger.LogInformation("GetISPAddress -> ISP address has been requested");
            await _loghub.SendLogInfoAsync(serviceName, "HTTPController -> GetISPAddress -> ISP address has been requested");

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

            if (string.Equals(outputString, _iSPAddressService.GetCurrentISPAddress(), StringComparison.CurrentCultureIgnoreCase))
            {
                _counterService.AddInternalISPCheckCounter();
            }
            else
            {
                _counterService.AddExternalISPCheckCounter();
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
                _logger.LogInformation("HTTPController -> GetISPAddress -> Success addres returned:{logInfo}", logInfo);
                await _loghub.SendLogInfoAsync(serviceName, $"GetISPAddress ->Success addres returned:{logInfo}");

                return outputString;
            }
            else
            {
                _logger.LogError("GetISPAddress -> Something went wrong output was: {outputString}", outputString);
                await _loghub.SendLogErrorAsync(serviceName, $"Something went wrong output was: {outputString}");

                return "What?!?";
            }

        }
    }
}
