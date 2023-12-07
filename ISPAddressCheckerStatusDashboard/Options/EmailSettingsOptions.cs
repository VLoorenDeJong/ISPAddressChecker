namespace ISPAddressCheckerStatusDashboard.Options
{
    public class EmailSettingsOptions
    {
        public string? EmailFromAddress { get; set; }
        public string? EmailToAddress { get; set; }
        public string? MailServer { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool EnableSsl { get; set; }
        public int SMTPPort { get; set; }
        public bool UseDefaultCredentials { get; set; }

        public class StandardAppsettingsValues
        {
            public const string EmailFromAddress = "EmailFromAddress";
            public const string EmailToAddress = "EmailToAddress";
            public const string EmailSubject = "YourEmailSubject";
            public const string MailServer = "MailServer";
            public const string UserName = "UserName";
            public const string Password = "Pa$$w0rd";
        }
    }
}
