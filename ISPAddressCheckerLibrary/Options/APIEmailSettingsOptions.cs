namespace ISPAddressChecker.Options
{
    public class DNSHostProvider
    {
        public string? Name { get; set; }
        public string? URL { get; set; }
    }

    public class APIEmailSettingsOptions : EmailSettingsOptions
    {
        public List<DNSHostProvider>? DNSHostProviders { get; set; }

        public string? EmailSubject { get; set; }

        public bool HeartbeatEmailEnabled { get; set; }
        public TimeSpan HeartbeatEmailTimeOfDay { get; set; }
        public DayOfWeek HeartbeatEmailDayOfWeek { get; set; }
        public int HeartbeatEmailIntervalDays { get; set; } = 7;
    }
}
