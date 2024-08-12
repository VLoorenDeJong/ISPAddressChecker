using ISPAddressChecker.Models;

namespace ISPAddressChecker.Interfaces
{
    public interface IAPIEmailService
    {
        SendEmailModel APIEmailDetails { get; }

        Task SendConfigErrorMail(string errorMessage);
        Task SendConfigSuccessMail(IISPAddressCounterService counterService);
        Task SendConnectionReestablishedEmail(string newISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval);
        Task SendCounterDifferenceEmail(IISPAddressCounterService counterService);
        Task SendDifferendISPAddressValuesEmail(Dictionary<string, string> externalISPAddressChecks, string oldISPAddress, IISPAddressCounterService counterService, double interval);
        Task SendExternalAPIExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        Task SendExternalAPIHTTPExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        Task<ActionReportModel> SendHeartBeatEmail(IISPAddressCounterService counterService, string oldISPAddress, string currentISPAddress, string newISPAddress, Dictionary<string, string> externalISPCheckResults, SendEmailModel sendEmailDetails, TimeSpan uptime);
        Task<ActionReportModel> SendISPAddressChangedEmail(string externalISPAddress, string oldISPAddress, IISPAddressCounterService counterService, double interval, SendEmailModel sendEmailDetails);
        Task SendISPAPIExceptionEmail(string exceptionType, string exceptionMessage);
        Task SendISPAPIHTTPExceptionEmail(string exceptionType, string exceptionMessage);
        Task SendNoISPAddressReturnedEmail(string oldISPAddress, IISPAddressCounterService counterService, double interval);
    }
}