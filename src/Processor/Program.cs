using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static void Main(string[] args)
    {
        var repo = args[0];
        var number = long.Parse(args[1]);
        var deployment = args[2];
        var headCommit = args[3];
        var mergeCommit = args[4];
        var installationId = long.Parse(args[5]);

        var client = new GitHubClient(repo, number, headCommit, mergeCommit, installationId);
        
        var tempDirectory = Directory.CreateTempSubdirectory();
    }
}
