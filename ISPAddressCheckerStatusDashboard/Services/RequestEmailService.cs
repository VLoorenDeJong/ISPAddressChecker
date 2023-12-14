using ISPAddressChecker.Helpers;
using ISPAddressChecker.Models;
using ISPAddressChecker.Models.Enums;
using ISPAddressChecker.SignalRHubs;
using ISPAddressChecker.SignalRHubs.Interfaces;
using ISPAddressCheckerStatusDashboard.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ISPAddressCheckerStatusDashboard.Services
{
    public class RequestEmailService : IRequestEmailService
    {
        private readonly IOpenAPIClient? _apiClient;
        private readonly ILogger<RequestEmailService> _logger;
        private readonly ILogHubService _loghub;

        public RequestEmailService(IOpenAPIClient openAPIClient
                                  , ILogger<RequestEmailService> logger
                                  , ILogHubService loghub)
        {
            _apiClient = openAPIClient;
            _logger = logger;
            _loghub = loghub;
        }

        public async Task<ActionReportModel> RequestEmail(SendEmailModel emailRequest)
        {
            ActionReportModel report = new();
            report.Success = false;
            report.Message = "Something went wrong";
            report.Id = emailRequest.Id;


            if (emailRequest is not null)
            {
                try
                {
                    _logger.LogInformation("RequestEmail -> E-mail type: {type}, Id: {id}, Email-Address: {address}, Valid: {valid}", emailRequest.EmailType.ToString(), emailRequest.Id, StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), emailRequest.EmailValidated);
                    _loghub.AddLogMessage(new LogEntryModel(LogType.Information,
                                                            "RequestEmailService",
                                                            $"RequestId: {emailRequest.Id}, RequestEmailService -> E-mail type: {Helpers.StringHelpers.GetEmailEnum(emailRequest.EmailType)},  Email-Address: {StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress)}, Valid: {emailRequest.EmailValidated}"));
                    report = await _apiClient!.ISPAddressCheckSendEmailAsync(emailRequest);

                }
                catch (Exception ex)
                {
                    _logger.LogError("RequestEmail -> Failed -> Message: {message}, Id: {id}, type: {type}", ex.Message, emailRequest.Id, emailRequest.EmailType.ToString());
                    _loghub.AddLogMessage(new LogEntryModel(LogType.Error,
                                                            "RequestEmailService",
                                                            $"RequestId: {emailRequest.Id},RequestEmailService -> Failed -> Message: {ex.Message}, type: {Helpers.StringHelpers.GetEmailEnum(emailRequest.EmailType)}"));
                }
            }
            return report;
        }
    }
}
