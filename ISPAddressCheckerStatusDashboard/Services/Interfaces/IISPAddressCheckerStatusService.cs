using ISPAddressCheckerStatusDashboard;

namespace ISPAddressChecker.Interfaces
{
    public interface IISPAddressCheckerStatusService
    {
        Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync();
    }
}