namespace ISPAdressChecker.SignalRHubs.Interfaces
{
    public interface IClock
    {
        Task ShowTime(DateTime currentTime);
    }
}
