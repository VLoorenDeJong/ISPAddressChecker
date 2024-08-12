namespace ISPAddressChecker.Options
{
    public class DashboardApplicationSettingsOptions
    {
        public bool ShowSignalRTestClock { get; set; }
        public string? APIUrl { get; set; }
        public int EmailCounterResetTimeInHours { get; set; }
        public double AppsettingsVersion { get; set; }
        public string? CreatorEmail { get; set; }

        public double ExpectedAppsettingsVersion = 1.1;


        public class AppsettingsSections
        {
            public const string ApplicationSettings = "ApplicationSettings";
            public const string EmailSettings = "EmailSettings";
        }
    }
}
