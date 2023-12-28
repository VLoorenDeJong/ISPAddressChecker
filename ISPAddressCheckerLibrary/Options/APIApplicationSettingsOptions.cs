namespace ISPAddressChecker.Options

{
    public class APIApplicationSettingsOptions
    {
        public string? APIEndpointURL { get; set; }
        public List<string?>? BackupAPIS { get; set; }
        public double ISPAddressCheckFrequencyInMinutes { get; set; }
        public string? DateTimeFormat { get; set; }
        public bool DashboardEnabled { get; set; }

        public double AppsettingsVersion { get; set; }
        public double ExpectedAppsettingsVersion  = 1.1;

       
    }
}
