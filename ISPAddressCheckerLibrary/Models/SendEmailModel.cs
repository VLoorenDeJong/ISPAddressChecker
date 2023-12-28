using System.ComponentModel.DataAnnotations;
using ISPAddressChecker.Models.Constants;

namespace ISPAddressChecker.Models
{
    public class SendEmailModel
    {
        
        public SendEmailModel()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 5);
        }

        public bool EmailValidated { get; set; }
        [Required]
        public string EmailAddress { get; set; } = string.Empty;
        public SendEmailTypeEnum EmailType { get; set; }
        public string Id { get; set; } 

    }
}
