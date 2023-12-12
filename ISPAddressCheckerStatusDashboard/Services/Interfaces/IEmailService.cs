namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IEmailService
    {
         Task SendConfigFailMail();
         Task SendConfigSuccessMail();
    }
}