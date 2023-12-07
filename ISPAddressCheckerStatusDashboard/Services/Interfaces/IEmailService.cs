namespace ISPAddressCheckerStatusDashboard.Services.Interfaces
{
    public interface IEmailService
    {
        void SendConfigFailMail();
        void SendConfigSuccessMail();
    }
}