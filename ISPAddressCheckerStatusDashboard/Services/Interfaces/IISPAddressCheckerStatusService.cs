namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IISPAddressCheckerStatusService
    {
        Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync();
    }
}