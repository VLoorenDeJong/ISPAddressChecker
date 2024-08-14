namespace ISPAddressChecker.Options
{
    public class DashboardApplicationSettingsOptions
    {
        public bool ShowSignalRTestClock { get; set; }
        public int EmailCounterResetTimeInHours { get; set; }
        public double AppsettingsVersion { get; set; }
        public string? CreatorEmail { get; set; }
        public string? APIBaseURL { get; set; }

        public double ExpectedAppsettingsVersion = 1.2;


        public class AppsettingsSections
        {
            public const string ApplicationSettings = "ApplicationSettings";
            public const string EmailSettings = "EmailSettings";
        }
    }
}
