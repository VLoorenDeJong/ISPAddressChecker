namespace ISPAddressChecker.Interfaces
{
    public interface IDashboardEmailService
    {
         Task SendConfigFailMail();
         Task SendConfigSuccessMail();
    }
}