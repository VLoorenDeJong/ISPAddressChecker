using System.Security.Cryptography.X509Certificates;

namespace ISPAddressChecker.Options
{
    public class ApplicationSettingsOptions
    {
        public string? APIEndpointURL { get; set; }
        public List<string?>? BackupAPIS { get; set; }
        public double ISPAddressCheckFrequencyInMinutes { get; set; }
        public string? DateTimeFormat { get; set; }
        public bool DashboardEnabled { get; set; }

        public double AppsettingsVersion { get; set; }
        public double ExpectedAppsettingsVersion  = 1.1;

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
        }

       
    }
}
