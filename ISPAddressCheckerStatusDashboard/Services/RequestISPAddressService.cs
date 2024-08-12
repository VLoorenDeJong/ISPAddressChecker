using ISPAddressChecker.Helpers;
using ISPAddressChecker.Interfaces;
using ISPAddressCheckerStatusDashboard;

namespace ISPAddressCheckerDashboard.Services
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
                url = await _apiClient!.ISPAddressCheckAPIWebEndpointURLAsync();
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                _logger.LogError("GetCHeckISPAddressEndpointURLAsync -> ISP address check URL Request Error:{message}", ex.Message);
                _logger.LogError("GetCHeckISPAddressEndpointURLAsync -> ISP address check URL Request: Is the API running?");
                url = $"Error: GetCHeckISPAddressEndpointURLAsync -> ISP address check URL Request Error:{ ex.Message}, is the API runn ";
            }
            catch (Exception ex)
            {
                _logger.LogError("GetCHeckISPAddressEndpointURLAsync -> ISP address check URL Request Error:{message}", ex.Message);
                url = $"Error: GetCHeckISPAddressEndpointURLAsync -> ISP address check URL Request Error:{ ex.Message} ";
            }

            return url;
        }

        public async Task<string> GetLogHubURLAsync()
        {
            string url = string.Empty;

            try
            {
                _logger.LogInformation("GetLogHubURLAsync -> Requesting LogHub URL");
                url = await _apiClient!.ISPAddressGetAPILoghubURLAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetLogHubURLAsync -> ISP address LogHubURL Request Error:{message}", ex.Message);
            }

            return url;
        }

        public async Task<string> GetClockhubURLAsync()
        {
            string url = string.Empty;

            try
            {
                _logger.LogInformation("GetClockhubURLAsync -> Requesting URL for LogHub");
                url = await _apiClient!.ISPAddressGetAPIClockhubURLAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetClockhubURLAsync -> LogHub URL Request Error:{message}", ex.Message);
            }

            return url;
        }
    }
}
