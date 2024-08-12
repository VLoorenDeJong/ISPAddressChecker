namespace ISPAddressChecker.Interfaces
{
    public interface IRequestISPAddressService
    {
        Task<string> GetCHeckISPAddressEndpointURLAsync();
        Task<string> GetClockhubURLAsync();
        Task<string> GetLogHubURLAsync();
    }
}