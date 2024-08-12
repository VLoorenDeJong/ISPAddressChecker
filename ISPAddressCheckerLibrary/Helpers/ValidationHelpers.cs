using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Constants;
using ISPAddressChecker.Options;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace ISPAddressChecker.Helpers
{
    public static class ValidationHelpers
    {

        public static bool EmailAddressIsValid(string? emailAddressToValidate)
        {
            bool isValid = true;

            if (!string.IsNullOrWhiteSpace(emailAddressToValidate))
            {
                isValid = !Regex.IsMatch(emailAddressToValidate, @"(@.*@)|(\.\.)")
                            &&
                          Regex.IsMatch(emailAddressToValidate!, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

                ;
            }
            else
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
