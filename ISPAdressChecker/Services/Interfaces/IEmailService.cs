using ISPAdressChecker.Models;

namespace ISPAdressChecker.Services.Interfaces
{
    public interface IEmailService
    {
        SendEmailModel APIEmailDetails { get; }

        void SendConfigErrorMail(string errorMessage);
        void SendConfigSuccessMail(IISPAdressCounterService counterService);
        void SendConnectionReestablishedEmail(string newISPAddress, string oldISPAddress, IISPAdressCounterService counterService, double interval);
        void SendCounterDifferenceEmail(IISPAdressCounterService counterService);
        void SendDifferendISPAdressValuesEmail(Dictionary<string, string> externalISPAdressChecks, string oldISPAddress, IISPAdressCounterService counterService, double interval);
        void SendExternalAPIExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        void SendExternalAPIHTTPExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        ActionReportModel SendHeartBeatEmail(IISPAdressCounterService counterService, string oldISPAddress, string currentISPAddress, string newISPAddress, Dictionary<string, string> externalISPCheckResults, SendEmailModel sendEmailDetails);
        ActionReportModel SendISPAddressChangedEmail(string externalISPAddress, string oldISPAddress, IISPAdressCounterService counterService, double interval, SendEmailModel sendEmailDetails);
        void SendISPAPIExceptionEmail(string exceptionType, string exceptionMessage);
        void SendISPAPIHTTPExceptionEmail(string exceptionType, string exceptionMessage);
        void SendNoISPAdressReturnedEmail(string oldISPAddress, IISPAdressCounterService counterService, double interval);
    }
}