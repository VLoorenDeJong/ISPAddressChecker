using System.Text;

namespace ISPAddressChecker.Helpers
{   
    public static class StringHelpers
    {
        public static string MakeISPAddressLogReady(string ISPAddress)
        {
            string output = string.Empty;
            if (!string.IsNullOrWhiteSpace(ISPAddress))
            {

                string[] octets = ISPAddress.Split('.');
                int secondOctetInt = int.Parse(octets[1]);
                string secondOctetString = secondOctetInt.ToString().PadRight(3, '0');
                string secondOctet = secondOctetString.Substring(0, secondOctetString.Length - 2);
                secondOctet += "**";

                octets[1] = secondOctet;



                // Ensure the last octet has 3 digits
                int lastOctet = int.Parse(octets[3]);
                string lastOctetString = lastOctet.ToString().PadRight(3, '0');

                // Remove last 2 digits of octed                            
                string lastOcted = lastOctetString.Substring(0, lastOctetString.Length - 2);

                // Append "**" to the masked string
                lastOcted += "**";

                // Replace the last octet in the IP address
                octets[3] = lastOcted;
                string maskedIpAddress = string.Join(".", octets);

                output = maskedIpAddress; // Output: "192.168.1.x**"
            }

            return output;
        }

        public static string MakeEmailAddressLogReady(string emailAddress)
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
