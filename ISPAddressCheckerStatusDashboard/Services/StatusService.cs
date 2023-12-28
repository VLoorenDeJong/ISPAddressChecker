using ISPAddressChecker.Models;
using ISPAddressChecker.Interfaces;

namespace ISPAddressCheckerDashboard.Services
{
    public class StatusService : IStatusService
    {
        public ISPAddressChecker.Models.ISPAddressCheckerStatusUpdateModel CurrentStatus { get; private set; } = new();

        public event Action? OnChange;

        private readonly IISPAddressCheckerStatusService _iSPStatusService;

        public StatusService(IISPAddressCheckerStatusService iSPStatusService)
        {
            _iSPStatusService = iSPStatusService;
        }

        public async Task GetStatus()
        {
            try
            {
                CurrentStatus = await _iSPStatusService.GetAPIStatusAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
