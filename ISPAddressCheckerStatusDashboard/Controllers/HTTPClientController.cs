using ISPAddressChecker.Models.Constants;
using ISPAddressCheckerStatusDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ISPAddressCheckerStatusDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ISPInfoController : ControllerBase
    {
        private readonly IHTTPClientControllerMessageService _messageService;

        public ISPInfoController(IHTTPClientControllerMessageService messageService)
        {
            _messageService = messageService;
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
                    ipAddress = address.MapToIPv4().ToString();
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