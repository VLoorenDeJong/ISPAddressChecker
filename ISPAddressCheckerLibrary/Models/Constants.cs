namespace ISPAddressChecker.Models.Constants
{
    public class EmailEnums
    {
        public const string Internal = "Internal";
        public const string HeartBeatEmail = "HeartBeat";
        public const string ISPAddressChanged = "ISPAddressChanged";
    }

    public class AppsettingsSections
    {
        public const string ApplicationSettings = "ApplicationSettings";
        public const string EmailSettings = "EmailSettings";
    }

    public class StandardAppsettingsValues
    {
        public const string APIEndpointURL = "ThisAPIEnpointURL";
        public const string DNSRecordHostProviderName = "HostingProviderName";
        public const string DNSRecordHostProviderURL = "HostingProviderURL";
        public const string EmailFromAddress = "EmailFromAddress";
        public const string EmailToAddress = "EmailToAddress";
        public const string EmailSubject = "YourEmailSubject";
        public const string MailServer = "MailServer";
        public const string UserName = "UserName";
        public const string Password = "Pa$$w0rd";
        public const string APIBaseURL = "YourAPIBaseURL";
    }
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

    public class BlazorEndpointURLS
    {
        public const string GetVisitorISPURL = "api/ISPInfo/GetVisitorISP";
    }

    public enum LogType
    {
        Information,
        Warning,
        Error,
        Debug
    }
    public enum SendEmailTypeEnum
    {
        Internal = 0,
        HeartBeatEmail,
        ISPAddressChanged
    }
}
