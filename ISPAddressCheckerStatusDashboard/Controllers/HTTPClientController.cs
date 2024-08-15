using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ISPAddressCheckerStatusDashboard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ISPInfoController : ControllerBase
    {
        [HttpGet("GetISPInfo")]
        public ActionResult<string> GetISPInfo()
        {
            var ipAddress = GetClientIpAddress(HttpContext);

            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("Unable to determine IP address.");
            }

            return Ok(ipAddress);
        }

        private string GetClientIpAddress(HttpContext context)
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