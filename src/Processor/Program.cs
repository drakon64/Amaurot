namespace Amaurot.Processor;

internal sealed class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static void Main(string[] args)
    {
        var repo = args[0];
        var commit = args[1];

        Console.Out.WriteLine($"Repository: {repo}");
        Console.Out.WriteLine($"Commit: {commit}");
    }
}
