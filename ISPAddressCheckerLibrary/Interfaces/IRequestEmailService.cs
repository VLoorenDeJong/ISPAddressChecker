using ISPAddressChecker.Models;

namespace ISPAddressChecker.Interfaces
{
    public interface IRequestEmailService
    {
        Task<ActionReportModel> RequestEmailAsync(SendEmailModel emailRequest);
    }
}