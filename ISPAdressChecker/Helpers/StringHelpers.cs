namespace ISPAddressChecker.Helpers
{
    public static class StringHelpers
    {

        public static string MakeISPAddressLogReady(string ISPAddress) 
        {
            string output = string.Empty;
            if (!string.IsNullOrWhiteSpace(ISPAddress)) {
                int secondToLastDotIndex = ISPAddress.LastIndexOf(".");
                output = $"{ISPAddress.Substring(0, secondToLastDotIndex + 2)}*";
            }

            return output;
        }
    }
}
