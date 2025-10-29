using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static async Task Main(string[] args)
    {
        var repo = args[0];
        var number = long.Parse(args[1]);
        var commit = args[2];
        var installationId = long.Parse(args[3]);

        await Console.Out.WriteLineAsync($"Repository: {repo}");
        await Console.Out.WriteLineAsync($"Number: {number}");
        await Console.Out.WriteLineAsync($"Commit: {commit}");

        await GitHubClient.ListPullRequestFiles(repo, number, installationId);
    }
}
