using ISPAdressChecker.Interfaces;

namespace ISPAdressChecker.Models
{
    public class StatusUpdateModel
    {
        public StatusUpdateModel()
        {

        }
        public StatusUpdateModel(IISPAdressCounterService ISPAdressCounterService, IStatusCounterService statusCounterService, ITimerService timerService)
        {

            ISPEndpointRequests = ISPAdressCounterService.GetISPEndpointRequestsCounter();
            ServiceRequestCounter = ISPAdressCounterService.GetServiceRequestCounter();
            ServiceCheckCounter = ISPAdressCounterService.GetServiceCheckCounter();
            ExternalServiceCheckCounter = ISPAdressCounterService.GetExternalServiceCheckCounter();
            FailedISPRequestCounter = ISPAdressCounterService.GetFailedISPRequestCounter();


            StatusUpdateRequested = statusCounterService.GetStatusUpdateRequested();

            Uptime = timerService.GetUptime();
        }
        public TimeSpan Uptime { get; }
        public int ISPEndpointRequests { get; }
        public int ServiceRequestCounter { get; }
        public int ServiceCheckCounter { get; }
        public int ExternalServiceCheckCounter { get; }
        public int FailedISPRequestCounter { get; }
        public int StatusUpdateRequested { get; }
    }
}
