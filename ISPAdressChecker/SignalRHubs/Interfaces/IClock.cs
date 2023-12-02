namespace ISPAddressChecker.SignalRHubs.Interfaces
{
    public interface IClock
    {
        Task ShowTime(DateTime currentTime);
    }
}
