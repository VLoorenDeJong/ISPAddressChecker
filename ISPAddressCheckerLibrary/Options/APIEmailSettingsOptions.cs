namespace ISPAddressChecker.Options
{
    public class APIEmailSettingsOptions : EmailSettingsOptions
    {
        public string? DNSRecordHostProviderName { get; set; }
        public string? DNSRecordHostProviderURL { get; set; }

        public string? EmailSubject { get; set; }

        public bool HeartbeatEmailEnabled { get; set; }
        public TimeSpan HeartbeatEmailTimeOfDay { get; set; }
        public DayOfWeek HeartbeatEmailDayOfWeek { get; set; }
        public int HeartbeatEmailIntervalDays { get; set; } = 7;
    }
}
