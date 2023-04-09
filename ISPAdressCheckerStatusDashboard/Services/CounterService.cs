using ISPAdressCheckerStatusDashboard.Services.Interfaces;

namespace ISPAdressCheckerStatusDashboard.Services
{
    public class CounterService : ICounterService
    {
        public int HeartbeatEmailRequestCounter = 10;
        public int ISPAddressChangedRequestCounter = 10;

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


        public void ResetCounters()
        {
            ISPAddressChangedRequestCounter = 10;
            HeartbeatEmailRequestCounter = 10;
        }

    }
}
