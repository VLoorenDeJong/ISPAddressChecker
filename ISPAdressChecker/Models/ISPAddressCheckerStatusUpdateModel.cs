using ISPAdressChecker.Services.Interfaces;
using System.Diagnostics;

namespace ISPAdressChecker.Models
{
    public class ISPAddressCheckerStatusUpdateModel
    {
        public ISPAddressCheckerStatusUpdateModel()
        {

        }

        public ISPAddressCheckerStatusUpdateModel(IISPAdressCounterService ISPAdressCounterService, IStatusCounterService statusCounterService, ITimerService timerService)
        {
            ISPEndpointRequests = ISPAdressCounterService.GetISPEndpointRequestsCounter();
            ServiceRequestCounter = ISPAdressCounterService.GetServiceRequestCounter();
            ServiceCheckCounter = ISPAdressCounterService.GetServiceCheckCounter();
            ExternalServiceCheckCounter = ISPAdressCounterService.GetExternalServiceCheckCounter();
            FailedISPRequestCounter = ISPAdressCounterService.GetFailedISPRequestCounter();

            StatusUpdateRequested = statusCounterService.GetStatusUpdateRequested();

            StartDateTime = timerService.GetStartDateTime();
        }

        public DateTimeOffset StartDateTime { get; }
        public int ISPEndpointRequests { get; }
        public int ServiceRequestCounter { get; }
        public int ServiceCheckCounter { get; }
        public int ExternalServiceCheckCounter { get; }
        public int FailedISPRequestCounter { get; }
        public int StatusUpdateRequested { get; }
    }
}
