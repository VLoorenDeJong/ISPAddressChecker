using ISPAdressChecker.Helpers;
using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{
    public class RequestISPAddressService : IRequestISPAddressService
    {
        private readonly IOpenAPIClient? _apiClient;
        private readonly ILogger<RequestISPAddressService> _logger;
        public RequestISPAddressService(IOpenAPIClient openAPIClient, ILogger<RequestISPAddressService> logger)
        {
            _apiClient = openAPIClient;
            _logger = logger;
        }

        public async Task<string> GetCHeckISPAddressEndpointURLAsync()
        {
            string url = string.Empty;

            try
            {
                _logger.LogInformation("GetCHeckISPAddressEndpointURLAsync -> Requesting URL for ISP address check");
                url = await _apiClient!.ISPAddressCheckAPIEndpointURLAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCHeckISPAddressEndpointURLAsync -> ISP address check URL Request Error:{message}", ex.Message);
            }

            return url;
        }
    }
}
