namespace ISPAddressChecker.Options
{
    public enum IPVersionPreference
    {
        IPv4,
        IPv6
    }

    public class DashboardApplicationSettingsOptions
    {
        public bool ShowSignalRTestClock { get; set; }
        public int EmailCounterResetTimeInHours { get; set; }
        public double AppsettingsVersion { get; set; }
        public string? CreatorEmail { get; set; }
        public string? APIBaseURL { get; set; }
        public IPVersionPreference IPVersionPreference { get; set; } = IPVersionPreference.IPv4;

        public double ExpectedAppsettingsVersion = 1.3;


        public class AppsettingsSections
        {
            public const string ApplicationSettings = "ApplicationSettings";
            public const string EmailSettings = "EmailSettings";
        }
    }
}
