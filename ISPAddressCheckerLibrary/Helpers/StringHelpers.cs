using System.Net; 
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ISPAddressChecker.Helpers
{   
    public static class StringHelpers
    {
        public static string MakeISPAddressLogReady(string ISPAddress)
        {
            string output = string.Empty;
            if (!string.IsNullOrWhiteSpace(ISPAddress))
            {
                if (IPAddress.TryParse(ISPAddress.Trim(), out IPAddress? parsedAddress)
                    && parsedAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // IPv6: show first 4 groups in full hex, mask last 4 groups
                    byte[] bytes = parsedAddress.GetAddressBytes();
                    output = string.Format("{0:x2}{1:x2}:{2:x2}{3:x2}:{4:x2}{5:x2}:{6:x2}{7:x2}:****:****:****:****",
                        bytes[0], bytes[1], bytes[2], bytes[3],
                        bytes[4], bytes[5], bytes[6], bytes[7]);
                }
                else
                {
                    // IPv4 address
                    string[] octets = ISPAddress.Trim().Split('.');
                    if (octets.Length == 4
                        && int.TryParse(octets[1], out int secondOctetInt)
                        && int.TryParse(octets[3], out int lastOctet))
                    {
                        string secondOctetString = secondOctetInt.ToString().PadRight(3, '0');
                        string secondOctet = secondOctetString.Substring(0, secondOctetString.Length - 2);
                        secondOctet += "**";
                        octets[1] = secondOctet;

                        // Ensure the last octet has 3 digits
                        string lastOctetString = lastOctet.ToString().PadRight(3, '0');

                        // Remove last 2 digits of octet
                        string lastOcted = lastOctetString.Substring(0, lastOctetString.Length - 2);

                        // Append "**" to the masked string
                        lastOcted += "**";

                        // Replace the last octet in the IP address
                        octets[3] = lastOcted;
                        output = string.Join(".", octets); // Output: "192.1**.1.x**"
                    }
                    else
                    {
                        output = ISPAddress;
                    }
                }
            }

            return output;
        }

        public static string MakeEmailAddressLogReady(string emailAddress, ILogger? logger)
        {

            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                try
                {

                    int atIndex = emailAddress.IndexOf('@');
                    string maskedAddress = emailAddress.Substring(0, Math.Min(atIndex, 2)).PadRight(5, '*') + emailAddress.Substring(atIndex);
                    return maskedAddress; // Outputs "ex****@example.com"
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "MakeEmailAddressLogReady -> Failed");
                }
            }

            return emailAddress;
        }

        public static string MakeHttpRequestHostDashboardReady(string host)
        {
            string output = "NoHostFound";

            if (!string.IsNullOrWhiteSpace(host))
            {
                if (host.Length > 4)
                {
                    // replcae last four characters with *
                    output = host.Substring(0, host.Length - 4) + "****";

                }
                else
                {
                    output = new string('*', host.Length);
                }

            }

            return output;
        }        
    }
}
