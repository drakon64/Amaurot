namespace Amaurot.Receiver;

internal static class Program
{
    internal static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();
        app.Run();
    }
}
