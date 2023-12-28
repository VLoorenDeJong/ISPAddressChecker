using ISPAddressChecker.Models;

namespace ISPAddressChecker.Interfaces
{
    public interface IISPAddressCheckerStatusService
    {
        Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync();
    }
}