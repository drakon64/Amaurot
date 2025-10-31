using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static void Main(string[] args)
    {
        var client = new GitHubClient(args[0], long.Parse(args[1]), long.Parse(args[3]));
    }
}
