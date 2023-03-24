using ISPAddressChecker.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace ISPAddressChecker.Services
{
    public class ISPAddressCounterService : IISPAddressCounterService
    {
        public ISPAddressCounterService()
        {
            ISPEndpointRequests = 0;
            ServiceRequestCounter = 0;
            ServiceCheckCounter = 1;
            FailedISPRequestCounter = 0;
            ExternalServiceCheckCounter = 0;
        }

        private int ISPEndpointRequests;
        private int ServiceRequestCounter;
        private int ServiceCheckCounter;
        private int ExternalServiceCheckCounter;
        private int FailedISPRequestCounter;

        public void AddISPEndpointRequests()
        {
            ISPEndpointRequests = ISPEndpointRequests + 1;
        }

        public int GetISPEndpointRequestsCounter()
        {
            return ISPEndpointRequests;
        }

        public void AddServiceRequestCounter()
        {
            ServiceRequestCounter++;
        }

        public int GetServiceRequestCounter()
        {
            return ServiceRequestCounter;
        }

        public void AddServiceCheckCounter()
        {
            ServiceCheckCounter++;
        }

        public int GetServiceCheckCounter()
        {
            return ServiceCheckCounter;
        }

        public void AddExternalServiceCheckCounter()
        {
            ExternalServiceCheckCounter++;
        }

        public int GetExternalServiceCheckCounter()
        {
            return ExternalServiceCheckCounter;
        }

        public void AddFailedISPRequestCounter()
        {
            FailedISPRequestCounter++;
        }

        public int GetFailedISPRequestCounter()
        {
            return FailedISPRequestCounter;
        }
        public void ResetFailedISPRequestCounter()
        {
            FailedISPRequestCounter = 0;
        }
    }
}