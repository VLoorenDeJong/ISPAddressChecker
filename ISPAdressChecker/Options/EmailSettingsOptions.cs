namespace ISPAdressChecker.Options
{
    public class EmailSettingsOptions
    {
        public string? EmailFromAdress { get; set; }
        public string? EmailToAdress { get; set; }
        public string? EmailSubject { get; set; }
        public string? MailServer { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool EnableSsl { get; set; }
        public int SMTPPort { get; set; }
        public bool UseDefaultCredentials { get; set; }

        public string? DNSRecordHostProviderName { get; set; }
        public string? DNSRecordHostProviderURL { get; set; }


        public TimeSpan HeatbeatEmailTimeOfDay { get; set; }
        public DayOfWeek HeatbeatEmailDayOfWeek { get; set; }

        public int HeatbeatEmailIntervalDays = 7;
    }
}
