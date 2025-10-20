namespace Amaurot.Receiver;

internal static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        var app = builder.Build();
        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
