using ISPAddressChecker.Models.Constants;
using ISPAddressChecker.Options;
using ISPAddressCheckerStatusDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace ISPAddressCheckerStatusDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ISPInfoController : ControllerBase
    {
        private readonly IHTTPClientControllerMessageService _messageService;
        private readonly DashboardApplicationSettingsOptions _settings;

        public ISPInfoController(IHTTPClientControllerMessageService messageService, IOptions<DashboardApplicationSettingsOptions> settings)
        {
            _messageService = messageService;
            _settings = settings.Value;
        }

        [HttpGet("GetVisitorISP")]
        public ActionResult<string> GetVisitorISP()
        {
            var ipAddress = GetVisitorISPAddress(HttpContext);

            //_logger.LogInformation("ISPAddressCheckerAPI.SignalRHubs -> {method} -> called", LogHubMethods.SendLogToClients);

            ISPAddressChecker.Models.LogEntryModel newLogEntry = new();
            newLogEntry.LogType = LogType.Information;
            newLogEntry.Service = $"Dashboard -> RequestEmail";
            newLogEntry.Message = $"RequestId: something";

            _messageService.SendLogMessageToDashboard("Green");

            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("Unable to determine IP address.");
            }

            return Ok(ipAddress);
        }

        private string GetVisitorISPAddress(HttpContext context)
        {
            string? ipAddress = context.Request.Headers.ContainsKey("X-Forwarded-For")
                ? context.Request.Headers["X-Forwarded-For"].ToString()
                : context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                if (IPAddress.TryParse(ipAddress, out var address))
                {
                    if (address.Equals(IPAddress.IPv6Loopback))
                    {
                        address = IPAddress.Loopback;
                    }

                    if (_settings.IPVersionPreference == IPVersionPreference.IPv4)
                    {
                        // Unwrap IPv4-mapped IPv6 (::ffff:1.2.3.4) to plain IPv4.
                        // Pure IPv6 clients have no IPv4 to return, so fall back to their IPv6 address.
                        ipAddress = address.IsIPv4MappedToIPv6
                            ? address.MapToIPv4().ToString()
                            : address.ToString();
                    }
                    else
                    {
                        // IPv6 preference: map plain IPv4 addresses to their IPv6 representation (::ffff:x.x.x.x).
                        ipAddress = address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                            ? address.MapToIPv6().ToString()
                            : address.ToString();
                    }
                }



                return ipAddress;


            }
            else
            {
                return string.Empty;
            }

        }
    }
}