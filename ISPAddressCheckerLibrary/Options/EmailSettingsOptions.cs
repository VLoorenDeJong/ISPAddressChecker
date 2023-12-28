namespace ISPAddressChecker.Options
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
    }
}
