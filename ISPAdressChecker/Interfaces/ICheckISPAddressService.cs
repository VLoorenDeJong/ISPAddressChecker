public interface ICheckISPAddressService
{
    Task GetISPAddressAsync();
    Task GetISPAddressFromBackupAPIs();
    Task HeartBeatCheck();
}