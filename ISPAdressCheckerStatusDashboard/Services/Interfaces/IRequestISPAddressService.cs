namespace ISPAdressCheckerStatusDashboard.Services.Interfaces
{
    public interface IRequestISPAddressService
    {
        Task<string> GetCHeckISPAddressEndpointURLAsync();
    }
}