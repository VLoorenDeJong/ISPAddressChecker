using ISPAddressChecker.Interfaces;

namespace ISPAddressChecker.Models
{
    public class StatusUpdateModel
    {
        public StatusUpdateModel()
        {

        }
        public StatusUpdateModel(IISPAddressCounterService ISPAddressCounterService, IStatusCounterService statusCounterService, ITimerService timerService)
        {
            ISPEndpointRequests = ISPAddressCounterService.GetISPEndpointRequestsCounter();
            ServiceRequestCounter = ISPAddressCounterService.GetServiceRequestCounter();
            ServiceCheckCounter = ISPAddressCounterService.GetServiceCheckCounter();
            ExternalServiceCheckCounter = ISPAddressCounterService.GetExternalServiceCheckCounter();
            FailedISPRequestCounter = ISPAddressCounterService.GetFailedISPRequestCounter();

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
