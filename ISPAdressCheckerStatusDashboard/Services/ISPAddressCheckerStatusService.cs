using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{

    public class ISPAddressCheckerStatusService : IISPAddressCheckerStatusService
    {
        private readonly TimeSpan? ispAddressCheckerLifeTime;

        private readonly IOpenAPIClient? _apiClient;
        public ISPAddressCheckerStatusService(IOpenAPIClient openAPIClient)
        {
            _apiClient = openAPIClient;
        }

        public async Task<ISPAddressCheckerStatusUpdateModel> GetAPIStatusAsync()
        {
            ISPAddressCheckerStatusUpdateModel status = new();

            try
            {
                status = await _apiClient!.GetStatusUpdateAsync();
            }
            catch (Exception ex)
            {

            }

            return status;
        }

    }
}
