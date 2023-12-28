using ISPAddressChecker.Interfaces;

namespace ISPAddressCheckerDashboard.Services
{
    public class CounterService : ICounterService
    {
        private int HeartbeatEmailRequestCounter = 10;
        private int ISPAddressChangedRequestCounter = 10;
        private int DashboardISPAddressRequestCounter;

        public int GetHeartbeatEmailRequestCounter()
        {
            return HeartbeatEmailRequestCounter;
        }
        public void SubtractHeartbeatEmailRequestCounter()
        {
            HeartbeatEmailRequestCounter--;
        }
        public int GetISPAddressChangedRequestCounter()
        {
            return ISPAddressChangedRequestCounter;
        }
        public void SubtractISPAddressChangedRequestCounter()
        {
            ISPAddressChangedRequestCounter--;
        }

        public void ResetEmailCounters()
        {
            ISPAddressChangedRequestCounter = 10;
            HeartbeatEmailRequestCounter = 10;
        }



        public int GetDashboardISPAddressRequestCounter()
        {
            return DashboardISPAddressRequestCounter;
        }
        public void AddDashboardISPAddressRequestCounter()
        {
            DashboardISPAddressRequestCounter++;
        }
    }
}
