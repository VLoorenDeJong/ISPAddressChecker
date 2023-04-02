namespace ISPAdressChecker.Helpers
{
    public static class StringHelpers
    {
        public static string MakeISPAddressLogReady(string ISPAdress) 
        {
            string output = string.Empty;
            if (!string.IsNullOrWhiteSpace(ISPAdress)) {

                string[] octets = ISPAdress.Split('.');

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
    }
}
