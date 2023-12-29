using ISPAddressChecker.Interfaces;

namespace ISPAddressChecker.Models
{
    public class ISPAddressCheckerStatusUpdateModel
    {
        public ISPAddressCheckerStatusUpdateModel()
        {

        }

        public ISPAddressCheckerStatusUpdateModel(IISPAddressCounterService ISPAddressCounterService, IStatusCounterService statusCounterService, ITimerService timerService, IISPAddressService iSPAddressService)
        {
            ISPEndpointRequests = ISPAddressCounterService.GetISPEndpointRequestsCounter();
            ServiceRequestCounter = ISPAddressCounterService.GetServiceRequestCounter();
            StatusUpdateRequested = statusCounterService.GetStatusUpdateRequested();
            StartDateTime = timerService.GetStartDateTime();
            CurrentISPAddress = iSPAddressService.GetCurrentISPAddress();
            RequestSuccessPercentage = ISPAddressCounterService.GetSuccessPercentage();

            InternalISPCheckCounter = ISPAddressCounterService.GetInternalISPCheckCounter();
            ExternalISPCheckCounter = ISPAddressCounterService.GetExternalISPCheckCounter();
        }

        public int ISPEndpointRequests { get; }
        public int ServiceRequestCounter { get; }
        public int InternalISPCheckCounter { get; }
        public int ExternalISPCheckCounter { get; }
        public int StatusUpdateRequested { get; }
        public DateTimeOffset StartDateTime { get; }
        public int RequestSuccessPercentage { get; set; }
        public string CurrentISPAddress { get; set; } = "ISPAddressCheckerStatusUpdateModel -> Default value";
    }
}
