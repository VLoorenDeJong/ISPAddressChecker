using ISPAdressChecker.Services.Interfaces;

namespace ISPAdressChecker.Models
{
    public class ISPAddressCheckerStatusUpdateModel
    {
        public ISPAddressCheckerStatusUpdateModel()
        {

        }

        public ISPAddressCheckerStatusUpdateModel(IISPAdressCounterService ISPAdressCounterService, IStatusCounterService statusCounterService, ITimerService timerService, IISPAddressService iSPAddressService)
        {
            ISPEndpointRequests = ISPAdressCounterService.GetISPEndpointRequestsCounter();
            ServiceRequestCounter = ISPAdressCounterService.GetServiceRequestCounter();
            StatusUpdateRequested = statusCounterService.GetStatusUpdateRequested();
            StartDateTime = timerService.GetStartDateTime();
            CurrentISPAddress = iSPAddressService.GetCurrentISPAddress();
            RequestSuccessPercentage = ISPAdressCounterService.GetSuccessPercentage();

            InternalISPCheckCounter = ISPAdressCounterService.GetInternalISPCheckCounter();
            ExternalISPCheckCounter = ISPAdressCounterService.GetExternalISPCheckCounter();
        }

        public int ISPEndpointRequests { get; }
        public int ServiceRequestCounter { get; }
        public int InternalISPCheckCounter { get; }
        public int ExternalISPCheckCounter { get; }
        public int StatusUpdateRequested { get; }
        public DateTimeOffset StartDateTime { get; }
        public int RequestSuccessPercentage { get; set; } = 83;
        public string CurrentISPAddress { get; set; } = "192.168.2.*!*";
    }
}
