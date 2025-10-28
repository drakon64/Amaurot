namespace Amaurot.Processor;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static void Main(string[] args)
    {
        var repo = args[0];
        var number = args[1];
        var commit = args[2];

        Console.Out.WriteLine($"Repository: {repo}");
        Console.Out.WriteLine($"Number: {number}");
        Console.Out.WriteLine($"Commit: {commit}");
    }
}
