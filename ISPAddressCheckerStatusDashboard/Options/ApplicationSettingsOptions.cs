namespace ISPAddressCheckerStatusDashboard.Options
{
    public class ApplicationSettingsOptions
    {
        public bool ShowSignalRTestClock { get; set; }
        public string? APIUrl { get; set; }
        public int EmailCounterResetTimeInHours { get; set; }
        public double AppsettingsVersion { get; set; }
        public double ExpectedAppsettingsVersion = 1.0;


        public class AppsettingsSections
        {
            public const string ApplicationSettings = "ApplicationSettings";
            public const string EmailSettings = "EmailSettings";
        }
    }
}
