using ISPAdressChecker.Services.Interfaces;

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

            //ToDo : calculate sucess percentage
            //ToDo get current ISP Address
        }

        public DateTimeOffset StartDateTime { get; }
        public int ISPEndpointRequests { get; }
        public int ServiceRequestCounter { get; }
        public int ServiceCheckCounter { get; }
        public int ExternalServiceCheckCounter { get; }
        public int FailedISPRequestCounter { get; }
        public int StatusUpdateRequested { get; }

        // Needed for sure:
        public int RequestSuccessPercentage { get; set; } = 83;
        public string CurrentISPAddress { get; set; } = "192.168.2.*!*";
    }
}
