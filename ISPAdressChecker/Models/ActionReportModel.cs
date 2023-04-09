namespace ISPAdressChecker.Models
{
    public class ActionReportModel
    {
        public string? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
