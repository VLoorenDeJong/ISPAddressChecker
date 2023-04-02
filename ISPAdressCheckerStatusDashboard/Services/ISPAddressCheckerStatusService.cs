using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{

    public class ISPAddressCheckerStatusService : IISPAddressCheckerStatusService
    {
        private readonly IOpenAPIClient? _apiClient;
        private readonly ILogger<ISPAddressCheckerStatusService> _logger;

        public ISPAddressCheckerStatusUpdateModel CurrentStatus { get; private set; }
        public event Action OnChange;

        public ISPAddressCheckerStatusService(IOpenAPIClient openAPIClient, ILogger<ISPAddressCheckerStatusService> logger)
        {
            _apiClient = openAPIClient;
            _logger = logger;
            CurrentStatus = new();
        }

        public async Task GetCurrentISPCheckerStatus()
        {
            CurrentStatus = await GetAPIStatusAsync();
            NotifyStateChanged();
        }

        private async Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync()
        {
            ISPAddressCheckerStatusUpdateModel status = new();

            try
            {
                status = await _apiClient!.GetStatusUpdateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAPIStatusAsync -> exception:{ex}", ex.Message);
            }

            return status;
        }
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
