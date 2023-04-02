using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{
    public class ApplicationService : IApplicationService, IHostedService
    {
        private IISPAddressCheckerStatusService _statusService;

        public ApplicationService(IISPAddressCheckerStatusService statusService)
        {
            _statusService = statusService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //await _statusService.GetAPICurrnetAPIStatus();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
