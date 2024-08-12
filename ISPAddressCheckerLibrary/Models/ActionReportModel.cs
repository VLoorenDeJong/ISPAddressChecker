using ISPAddressChecker.Models.Constants;

namespace ISPAddressChecker.Models
{
    public class ActionReportModel
    {
        public ActionReportModel()
        {
            
        }

        public ActionReportModel(SendEmailModel emailDetails)
        {
            Id = emailDetails.Id;
            SendEmailTypeEnum = emailDetails.EmailType;
        }

        public string? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public SendEmailTypeEnum SendEmailTypeEnum { get; set; }
    }
}
