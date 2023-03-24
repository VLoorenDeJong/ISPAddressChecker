using ISPAddressChecker.Interfaces;

namespace ISPAddressChecker.Services
{
    public class StatusCounterService : IStatusCounterService
    {
        public StatusCounterService()
        {

        }

        private int StatusUpdateRequested;


        public void AddStatusUpdateRequested()
        {
            StatusUpdateRequested = StatusUpdateRequested + 1;
        }

        public int GetStatusUpdateRequested()
        {
            return StatusUpdateRequested;
        }
    }
}
