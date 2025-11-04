using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static async Task Main(string[] args)
    {
        var action = args[0];
        var repo = args[1];
        var number = long.Parse(args[2]);
        var deployment = args[3];
        var headCommit = args[4];
        var mergeCommit = args[5];
        var installationId = long.Parse(args[6]);

        var githubClient = new GitHubClient(repo, number, headCommit, mergeCommit, installationId);
        var directory = await githubClient.DownloadRepositoryArchive();
    }
}
