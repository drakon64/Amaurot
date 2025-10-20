namespace Amaurot.Receiver;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();
        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
