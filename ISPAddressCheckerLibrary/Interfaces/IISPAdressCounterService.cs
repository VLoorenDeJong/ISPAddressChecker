namespace ISPAddressChecker.Interfaces
{
    public interface IISPAddressCounterService
    {
        void AddExternalISPCheckCounter();
        void AddExternalServiceUseCounter();
        void AddFailedISPRequestCounter();
        void AddInternalISPCheckCounter();
        void AddISPEndpointRequests();
        void AddServiceCheckCounter();
        void AddServiceRequestCounter();
        void AddSuccessFullRequestsCounter();
        int GetExternalISPCheckCounter();
        int GetExternalServiceUsekCounter();
        int GetFailedISPRequestCounter();
        int GetInternalISPCheckCounter();
        int GetISPEndpointRequestsCounter();
        int GetServiceCheckCounter();
        int GetServiceRequestCounter();
        int GetSuccessFullRequestsCounter();
        int GetSuccessPercentage();
        void ResetFailedISPRequestCounter();
    }
}