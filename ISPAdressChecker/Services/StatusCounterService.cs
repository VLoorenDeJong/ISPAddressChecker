using ISPAdressChecker.Services.Interfaces;

namespace ISPAdressChecker.Services
{
    public class StatusCounterService : IStatusCounterService
    {
        private int StatusUpdateRequested;
        private int startDateRequested;
        private int iSPAddressCheckIntervalRequested;

        public StatusCounterService()
        {

        }



        public void AddStatusUpdateRequested()
        {
            StatusUpdateRequested = StatusUpdateRequested + 1;
        }

        public int GetStatusUpdateRequested()
        {
            return StatusUpdateRequested;
        }

        public void AddStartdateRequested()
        {
            startDateRequested = startDateRequested + 1;
        }

        public int GetStartdateRequested()
        {
            return startDateRequested;
        }

        public void AddISPAddressCheckIntervalRequested()
        {
            iSPAddressCheckIntervalRequested = iSPAddressCheckIntervalRequested + 1;
        }

        public int GetISPAddressCheckIntervalRequested()
        {
            return iSPAddressCheckIntervalRequested;
        }
    }
}
