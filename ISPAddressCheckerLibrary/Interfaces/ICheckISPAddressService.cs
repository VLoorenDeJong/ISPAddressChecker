namespace ISPAddressChecker.Interfaces
{
    public interface ICheckISPAddressService
    {
        Task GetISPAddressAsync();
        Task GetISPAddressFromBackupAPIs(bool heartBeatCheck);
        Task HeartBeatCheck(TimeSpan uptime);
    }
}