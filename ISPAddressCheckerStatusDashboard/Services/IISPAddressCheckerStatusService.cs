using ISPAddressCheckerStatusDashboard;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public interface IISPAddressCheckerStatusService
    {
        Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync();
    }
}