namespace ISPAdressChecker.Models.Enums
{
    public class Constants
    {
        public class ClockHubMethods
        {
            public const string ShowTime = "ShowTime";
        }

        public class LogHubMethods
        {
            public const string SendLogToClients = "SendLogToClients";
        }
        public class SignalRHubUrls
        {
            public const string ClockHubURL = "/hubs/clock";
            public const string LogHubURL = "/hubs/log";
        }
    }
}
