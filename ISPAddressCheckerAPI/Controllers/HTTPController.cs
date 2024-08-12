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
        [HttpGet("UserISP", Name = "GetUserISP")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetUserISP()
        {
            _logger.LogInformation("GetUserISP -> ISP address has been requested");
            await _loghub.SendLogInfoAsync(serviceName, "HTTPController -> GetUserISP -> ISP address has been requested");

            string? ipAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString();

            // Check if it's an IPv6 address and convert to IPv4 format if necessary
            if (!string.IsNullOrWhiteSpace(ipAddress) && ipAddress.Contains(":"))
            {
                ipAddress = HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            }

            // Check for the X-Forwarded-For header to get the client IP address behind the proxy
            string? xForwardedForHeader = HttpContext?.Request?.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(xForwardedForHeader))
            {
                string[] addresses = xForwardedForHeader.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (addresses.Length > 0)
                {
                    ipAddress = addresses[0].Trim();
                }
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                _logger.LogError("GetUserISP -> Unable to determine IP address");
                await _loghub.SendLogErrorAsync(serviceName, "GetUserISP -> Unable to determine IP address");
                return BadRequest("Unable to determine IP address");
            }

            // Call an external API to get the ISP information
            string ispInfo = await GetISPInfoAsync(ipAddress);

            if (!string.IsNullOrEmpty(ispInfo))
            {
                _logger.LogInformation("GetUserISP -> Success, ISP information returned: {ispInfo}", ispInfo);
                await _loghub.SendLogInfoAsync(serviceName, $"GetUserISP -> Success, ISP information returned: {ispInfo}");
                return Ok(ispInfo);
            }
            else
            {
                _logger.LogError("GetUserISP -> Failed to retrieve ISP information for IP: {ipAddress}", ipAddress);
                await _loghub.SendLogErrorAsync(serviceName, $"GetUserISP -> Failed to retrieve ISP information for IP: {ipAddress}");
                return BadRequest("Failed to retrieve ISP information");
            }
        }

        private async Task<string> GetISPInfoAsync(string ipAddress)
        {
            using (HttpClient client = new HttpClient())
            {
                // Replace with the actual URL of the IP geolocation API you are using
                string apiUrl = $"https://api.ipgeolocation.io/ipgeo?apiKey=YOUR_API_KEY&ip={ipAddress}";

                try
                {
                    var response = await client.GetStringAsync(apiUrl);
                    var jsonDocument = JsonDocument.Parse(response);
                    if (jsonDocument.RootElement.TryGetProperty("isp", out JsonElement ispElement))
                    {
                        return ispElement.GetString();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("GetISPInfoAsync -> Exception: {ex}", ex);
                }
            }

            return string.Empty;
        }

    }
}
