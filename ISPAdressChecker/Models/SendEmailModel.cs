using ISPAdressChecker.Models.Enums;

namespace ISPAdressChecker.Models
{
    public class SendEmailModel
    {

        public SendEmailModel()
        {
            Id = Guid.NewGuid().ToString();
        }
        public bool EmailValidated { get; set; }
        public string? EmailAddress { get; set; }
        public SendEmailTypeEnum EmailType { get; set; }
        public string Id { get; set; } 

    }
}
