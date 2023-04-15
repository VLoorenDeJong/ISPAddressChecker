namespace MyApplication
{
    public interface IMyHub
    {
        Task SendAsync(string user, string message);
    }
}