namespace ISPAddressChecker.Interfaces
{
    public interface IEmailService
    {
        void SendConfigErrorMail(string errorMessage);
        void SendConfigSuccessMail(IISPAddressCounterService counterService);
        void SendConnectionReestablishedEmail(string newISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval);
        void SendCounterDifferenceEmail(IISPAddressCounterService counterService);
        void SendDifferendISPAddressValuesEmail(Dictionary<string, string> externalISPAddressChecks, string oldISPAddress, IISPAddressCounterService counterService, double interval);
        void SendExternalAPIExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        void SendExternalAPIHTTPExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        void SendHeartBeatEmail(IISPAddressCounterService counterService, string oldISPAddress, string currentISPAddress, string newISPAddress, Dictionary<string, string> externalISPCheckResults);
        void SendISPAddressChangedEmail(string externalISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval);
        void SendISPAPIExceptionEmail(string exceptionType, string exceptionMessage);
        void SendISPAPIHTTPExceptionEmail(string exceptionType, string exceptionMessage);
        void SendNoISPAddressReturnedEmail(string oldISPAddress, IISPAddressCounterService counterService, double interval);
    }
}