
using ISPAddressChecker.Interfaces;

namespace ISPAddressChecker.Services
{
    public class ISPAddressService : IISPAddressService
    {
        // IPv4
        private string CurrentISPAddress = string.Empty;
        private string NewISPAddress = string.Empty;
        private string OldISPAddress = string.Empty;
        private string ExternalISPAddress = string.Empty;

        // IPv6
        private string CurrentIPv6Address = string.Empty;
        private string NewIPv6Address = string.Empty;
        private string OldIPv6Address = string.Empty;
        private string ExternalIPv6Address = string.Empty;

        // ── IPv4 ────────────────────────────────────────────────────────────────

        public void SetCurrentISPAddress(string currentISPAddress)
        {
            if (!string.IsNullOrWhiteSpace(currentISPAddress))
            {
                CurrentISPAddress = currentISPAddress;
            };
        }
        public string GetCurrentISPAddress()
        {
            return CurrentISPAddress;
        }
        public void ClearCurrentISPAddress()
        {
            CurrentISPAddress = string.Empty;
        }

        public void SetNewISPAddress(string newISPAddress)
        {
            if (!string.IsNullOrWhiteSpace(newISPAddress))
            {
                NewISPAddress = newISPAddress;
            };
        }
        public string GetNewISPAddress()
        {
            return NewISPAddress;
        }
        public void ClearNewISPAddress()
        {
            NewISPAddress = string.Empty;
        }

        public void SetOldISPAddress(string oldISPAddress)
        {
            if (!string.IsNullOrWhiteSpace(oldISPAddress))
            {
                OldISPAddress = oldISPAddress;
            };
        }
        public string GetOldISPAddress()
        {
            return OldISPAddress;
        }
        public void ClearOldISPAddress()
        {
            OldISPAddress = string.Empty;
        }

        public void SetExternalISPAddress(string externalISPAddress)
        {
            if (!string.IsNullOrWhiteSpace(externalISPAddress))
            {
                ExternalISPAddress = externalISPAddress;
            };
        }
        public string GetExternalISPAddress()
        {
            return ExternalISPAddress;
        }
        public void ClearExternalISPAddress()
        {
            ExternalISPAddress = string.Empty;
        }

        // ── IPv6 ────────────────────────────────────────────────────────────────

        public void SetCurrentIPv6Address(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                CurrentIPv6Address = address;
            };
        }
        public string GetCurrentIPv6Address()
        {
            return CurrentIPv6Address;
        }
        public void ClearCurrentIPv6Address()
        {
            CurrentIPv6Address = string.Empty;
        }

        public void SetNewIPv6Address(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                NewIPv6Address = address;
            };
        }
        public string GetNewIPv6Address()
        {
            return NewIPv6Address;
        }
        public void ClearNewIPv6Address()
        {
            NewIPv6Address = string.Empty;
        }

        public void SetOldIPv6Address(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                OldIPv6Address = address;
            };
        }
        public string GetOldIPv6Address()
        {
            return OldIPv6Address;
        }
        public void ClearOldIPv6Address()
        {
            OldIPv6Address = string.Empty;
        }

        public void SetExternalIPv6Address(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                ExternalIPv6Address = address;
            };
        }
        public string GetExternalIPv6Address()
        {
            return ExternalIPv6Address;
        }
        public void ClearExternalIPv6Address()
        {
            ExternalIPv6Address = string.Empty;
        }
    }
}
