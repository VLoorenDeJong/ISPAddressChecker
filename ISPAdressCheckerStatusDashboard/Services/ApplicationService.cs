using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{
    public class ApplicationService : IApplicationService, IHostedService
    {
        private readonly IStatusService _statusService;

        public ApplicationService(IStatusService statusService)
        {
            _statusService = statusService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
