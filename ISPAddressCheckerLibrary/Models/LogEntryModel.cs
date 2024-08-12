using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Constants;

namespace ISPAddressChecker.Models
{
    public class LogEntryModel
    {
        public LogEntryModel()
        {
            Time = DateTimeOffset.Now;
            Id = Guid.NewGuid().ToString("N");
        }
        public LogEntryModel(LogType logType, string ServiceName, string message)
        {
            Time = DateTimeOffset.Now;
            LogType = logType;
            Service = ServiceName;
            Message = message;
            Id = Guid.NewGuid().ToString("N");
        }

        public LogType  LogType { get; set; }
        public DateTimeOffset Time { get; set; } 
        public string? Service { get; set; }
        public string? Message { get; set; }
        public string? Id { get; set; }
    }
}
