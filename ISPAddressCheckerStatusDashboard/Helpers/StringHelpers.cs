using ISPAddressCheckerStatusDashboard.Models;

namespace ISPAddressCheckerStatusDashboard.Helpers
{
    public static class StringHelpers
    {
        public static string GetEmailEnum(SendEmailTypeEnum emailEnum)
        {
            switch (emailEnum)
            {
                case SendEmailTypeEnum._0:
                    return EmailEnums.Internal;
                case SendEmailTypeEnum._1:
                    return EmailEnums.HeartBeatEmail;
                case SendEmailTypeEnum._2:
                    return EmailEnums.ISPAddressChanged;
                default:
                    return emailEnum.ToString();
            }
        }
    }
}
