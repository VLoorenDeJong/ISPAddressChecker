using ISPAdressChecker.Models;

namespace ISPAdressChecker.Services.Interfaces
{
    public interface IEmailService
    {
        SendEmailModel APIEmailDetails { get; }

        Task SendConfigErrorMail(string errorMessage);
        Task SendConfigSuccessMail(IISPAdressCounterService counterService);
        Task SendConnectionReestablishedEmail(string newISPAddress, string oldISPAddress, IISPAdressCounterService counterService, double interval);
        Task SendCounterDifferenceEmail(IISPAdressCounterService counterService);
        Task SendDifferendISPAdressValuesEmail(Dictionary<string, string> externalISPAdressChecks, string oldISPAddress, IISPAdressCounterService counterService, double interval);
        Task SendExternalAPIExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        Task SendExternalAPIHTTPExceptionEmail(string APIUrl, string exceptionType, string exceptionMessage);
        Task<ActionReportModel> SendHeartBeatEmail(IISPAdressCounterService counterService, string oldISPAddress, string currentISPAddress, string newISPAddress, Dictionary<string, string> externalISPCheckResults, SendEmailModel sendEmailDetails);
        Task<ActionReportModel> SendISPAddressChangedEmail(string externalISPAddress, string oldISPAddress, IISPAdressCounterService counterService, double interval, SendEmailModel sendEmailDetails);
        Task SendISPAPIExceptionEmail(string exceptionType, string exceptionMessage);
        Task SendISPAPIHTTPExceptionEmail(string exceptionType, string exceptionMessage);
        Task SendNoISPAdressReturnedEmail(string oldISPAddress, IISPAdressCounterService counterService, double interval);
    }
}