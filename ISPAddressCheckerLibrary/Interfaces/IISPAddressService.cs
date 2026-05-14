namespace ISPAddressChecker.Interfaces
{
    public interface IISPAddressService
    {
        // IPv4
        void ClearCurrentISPAddress();
        void ClearExternalISPAddress();
        void ClearNewISPAddress();
        void ClearOldISPAddress();
        string GetCurrentISPAddress();
        string GetExternalISPAddress();
        string GetNewISPAddress();
        string GetOldISPAddress();
        void SetCurrentISPAddress(string currentISPAddress);
        void SetExternalISPAddress(string externalISPAddress);
        void SetNewISPAddress(string newISPAddress);
        void SetOldISPAddress(string oldISPAddress);

        // IPv6
        void ClearCurrentIPv6Address();
        void ClearExternalIPv6Address();
        void ClearNewIPv6Address();
        void ClearOldIPv6Address();
        string GetCurrentIPv6Address();
        string GetExternalIPv6Address();
        string GetNewIPv6Address();
        string GetOldIPv6Address();
        void SetCurrentIPv6Address(string address);
        void SetExternalIPv6Address(string address);
        void SetNewIPv6Address(string address);
        void SetOldIPv6Address(string address);
    }
}