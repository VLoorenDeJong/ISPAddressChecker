using ISPAdressChecker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ISPAdressChecker.Models
{
    public class SendEmailModel
    {

        public SendEmailModel()
        {
            Id = Guid.NewGuid().ToString("N");
        }
        public bool EmailValidated { get; set; }
        [Required]
        public string EmailAddress { get; set; } = string.Empty;
        public SendEmailTypeEnum EmailType { get; set; }
        public string Id { get; set; } 

    }
}
