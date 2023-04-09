namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface IISPAddressCheckerStatusService
    {
        Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync();
    }
}