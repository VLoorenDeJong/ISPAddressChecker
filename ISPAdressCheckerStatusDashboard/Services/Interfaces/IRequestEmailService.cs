namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface IRequestEmailService
    {
        Task<ActionReportModel> RequestEmail(SendEmailModel emailRequest);
    }
}