using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{
    public class ApplicationService : IApplicationService, IHostedService
    {
        public ApplicationService()
        {
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
