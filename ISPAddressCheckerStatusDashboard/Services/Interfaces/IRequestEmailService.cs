
using ISPAddressCheckerStatusDashboard;

namespace ISPAddressChecker.Interfaces
{
    public interface IRequestEmailService
    {
        Task<ActionReportModel> RequestEmailAsync(SendEmailModel emailRequest);
    }
}