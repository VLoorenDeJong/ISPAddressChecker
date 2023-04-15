namespace ISPAdressChecker.Models
{
    public class LogEntryModel
    {
        public LogEntryModel()
        {
            
        }
        public LogEntryModel(string ServiceName, string message)
        {
            EntryDate = DateTimeOffset.Now;
            Service = ServiceName;
            Message = message;
        }

        public DateTimeOffset EntryDate { get; set; } 
        public string Service { get; set; }
        public string Message { get; set; }
    }
}
