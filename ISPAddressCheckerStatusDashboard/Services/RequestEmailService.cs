using ISPAddressChecker.Helpers;
using ISPAddressChecker.Interfaces;
using ISPAddressChecker.Models;

namespace ISPAddressCheckerDashboard.Services
{
    public class RequestEmailService : IRequestEmailService
    {
        private readonly IOpenAPIClient? _apiClient;
        private readonly ILogger<RequestEmailService> _logger;

        public RequestEmailService(IOpenAPIClient openAPIClient
                                  , ILogger<RequestEmailService> logger
                                  )
        {
            _apiClient = openAPIClient;
            _logger = logger;
        }

        public async Task<ActionReportModel> RequestEmailAsync(SendEmailModel emailRequest)
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
                    report = await _apiClient!.ISPAddressCheckSendEmailAsync(emailRequest);

                }
                catch (Exception ex)
                {
                    _logger.LogError("RequestEmail -> Failed -> Message: {message}, Id: {id}, type: {type}", ex.Message, emailRequest.Id, emailRequest.EmailType.ToString());
                }
            }
            return report;
        }

        public async Task<ISPAddressChecker.Models.ActionReportModel> RequestEmailAsync(ISPAddressChecker.Models.SendEmailModel emailRequest)
        {
            ISPAddressChecker.Models.ActionReportModel report = new();
            report.Success = false;
            report.Message = "Something went wrong";
            report.Id = emailRequest.Id;


            if (emailRequest is not null)
            {
                try
                {
                    _logger.LogInformation("RequestEmail -> E-mail type: {type}, Id: {id}, Email-Address: {address}, Valid: {valid}", emailRequest.EmailType.ToString(), emailRequest.Id, StringHelpers.MakeEmailAddressLogReady(emailRequest.EmailAddress), emailRequest.EmailValidated);
                   // report = await _apiClient!.ISPAddressCheckSendEmailAsync(emailRequest);

                }
                catch (Exception ex)
                {
                    _logger.LogError("RequestEmail -> Failed -> Message: {message}, Id: {id}, type: {type}", ex.Message, emailRequest.Id, emailRequest.EmailType.ToString());
                }
            }
            return report;
        }
    }
}
