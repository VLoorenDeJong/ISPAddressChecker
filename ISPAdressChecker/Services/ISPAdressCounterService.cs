using ISPAdressChecker.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ISPAdressChecker.Services
{
    public class ISPAdressCounterService : IISPAdressCounterService
    {
        public ISPAdressCounterService()
        {
            ISPEndpointRequests = 0;
            ServiceRequestCounter = 0;
            ServiceCheckCounter = 1;
            FailedISPRequestCounter = 0;
            ExternalServiceCheckCounter = 0;
            TotalFailedISPRequestCounter = 0;
            SuccessFullRequestsCounter = 0;
            SuccessPercentage = 100;
        }

        private int ISPEndpointRequests;
        private int ServiceRequestCounter;
        private int ServiceCheckCounter;
        private int ExternalServiceCheckCounter;
        private int FailedISPRequestCounter;
        private int TotalFailedISPRequestCounter;
        private int SuccessFullRequestsCounter;
        private int SuccessPercentage;

        private int CalculateSuccessPercentage()
        {
            // Testing code:
            //TotalFailedISPRequestCounter = 1;
            //SuccessFullRequestsCounter = 1;

            int percentage = 100;

            if (TotalFailedISPRequestCounter != 0)
            {
                int totalCalls = TotalFailedISPRequestCounter + SuccessFullRequestsCounter;
                percentage = (int)Math.Round((double)SuccessFullRequestsCounter / totalCalls * 100);
            }

            return percentage;
        }

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

        public void AddSuccessFullRequestsCounter()
        {
            SuccessFullRequestsCounter++;
            SuccessPercentage = CalculateSuccessPercentage();
        }

        public int GetSuccessFullRequestsCounter()
        {
            return SuccessFullRequestsCounter;
        }


        public void AddFailedISPRequestCounter()
        {
            FailedISPRequestCounter++;
            TotalFailedISPRequestCounter++;
            SuccessPercentage = CalculateSuccessPercentage();
        }

        public int GetFailedISPRequestCounter()
        {
            return FailedISPRequestCounter;
        }
        public void ResetFailedISPRequestCounter()
        {
            FailedISPRequestCounter = 0;
        }

        public int GetSuccessPercentage()
        {
            SuccessPercentage = CalculateSuccessPercentage();
            return SuccessPercentage;
        }
    }
}