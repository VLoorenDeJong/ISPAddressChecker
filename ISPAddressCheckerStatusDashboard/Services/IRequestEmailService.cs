using ISPAddressCheckerStatusDashboard;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public interface IRequestEmailService
    {
        Task<ActionReportModel> RequestEmailAsync(SendEmailModel emailRequest);
    }
}