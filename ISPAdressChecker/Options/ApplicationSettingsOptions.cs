using System.Security.Cryptography.X509Certificates;

namespace ISPAdressChecker.Options
{
    public class ApplicationSettingsOptions
    {
        public string? APIEndpointURL { get; set; }
        public List<string?>? BackupAPIS { get; set; }
        public double TimeIntervalInMinutes { get; set; }
        public string? DateTimeFormat { get; set; }
        public bool EnableDashboardAccess { get; set; }

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
            public const string EmailFromAdress = "EmailFromAdress";
            public const string EmailToAdress = "EmailToAdress";
            public const string EmailSubject = "YourEmailSubject";
            public const string MailServer = "MailServer";
            public const string UserName = "UserName";
            public const string Password = "Pa$$w0rd";
        }
    }
}
