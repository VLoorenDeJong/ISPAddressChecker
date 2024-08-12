using ISPAddressChecker.Interfaces;

namespace ISPAddressChecker.Services
{
    public class StatusCounterService : IStatusCounterService
    {
        private int StatusUpdateRequested;
        private int startDateRequested;
        private int iSPAddressCheckIntervalRequested;
        private int HeartbeatEmailRequested;
        private int ISPAddressChangedEmailRequested;

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

        public void AddISPHeartbeatEmailRequested()
        {
            HeartbeatEmailRequested = HeartbeatEmailRequested + 1;
        }
        public int GetISPHeartbeatEmailRequested()
        {
            return HeartbeatEmailRequested;
        }

        public void AddISPISPAddressChangedEmailRequested()
        {
            ISPAddressChangedEmailRequested = ISPAddressChangedEmailRequested + 1;
        }
        public int GetISPISPAddressChangedEmailRequested()
        {
            return ISPAddressChangedEmailRequested;
        }
    }
}
