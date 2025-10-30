using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor;

internal static class Program
{
    private static readonly HttpClient HttpClient = new();

    private static async Task Main(string[] args)
    {
        var repo = args[0];
        var number = long.Parse(args[1]);
        var commit = args[2];

        await Console.Out.WriteLineAsync($"Repository: {repo}");
        await Console.Out.WriteLineAsync($"Number: {number}");
        await Console.Out.WriteLineAsync($"Commit: {commit}");

        var client = new GitHubClient(HttpClient, long.Parse(args[3]));
    }
}
