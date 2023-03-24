namespace ISPAdressChecker.Helpers
{
    public static class StringHelpers
    {

        public static string MakeISPAddressLogReady(string ISPAdress) 
        {
            string output = string.Empty;
            if (!string.IsNullOrWhiteSpace(ISPAdress)) {
                int secondToLastDotIndex = ISPAdress.LastIndexOf(".");
                output = $"{ISPAdress.Substring(0, secondToLastDotIndex + 2)}*";
            }

            return output;
        }
    }
}
