namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IRequestEmailService
    {
        Task<ActionReportModel> RequestEmailAsync(SendEmailModel emailRequest);
    }
}