namespace ISPAddressCheckerStatusDashboard.Options
{
    public class ApplicationSettingsOptions
    {
        public bool ShowSignalRTestClock { get; set; }
        public string? APIUrl { get; set; }
        public int EmailCounterResetTimeInHours { get; set; }


        public class AppsettingsSections
        {
            public const string ApplicationSettings = "ApplicationSettings";
        }
    }
}
