using ISPAddressChecker.Helpers;
using ISPAddressChecker.Interfaces;

namespace ISPAddressCheckerDashboard.Services
{

    public class ISPAddressCheckerStatusService : IISPAddressCheckerStatusService
    {
        private readonly IOpenAPIClient? _apiClient;
        private readonly ILogger<ISPAddressCheckerStatusService> _logger;

        public ISPAddressCheckerStatusService(IOpenAPIClient openAPIClient, ILogger<ISPAddressCheckerStatusService> logger)
        {
            _apiClient = openAPIClient;
            _logger = logger;
        }

        public async Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync()
        {
            ISPAddressCheckerStatusUpdateModel status = new();
            try
            {
                _logger.LogInformation("GetAPIStatusAsync -> Requesting API status");
                status = await _apiClient!.GetStatusUpdateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetAPIStatusAsync -> exception:{ex}", ex.Message);
            }

            return status;
        }

        Task<ISPAddressChecker.Models.ISPAddressCheckerStatusUpdateModel> IISPAddressCheckerStatusService.GetAPIStatusAsync()
        {
            throw new NotImplementedException();
        }
    }
}
