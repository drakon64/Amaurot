using System.Text;
using System.Text.Json;

using Amaurot.Processor.Client.GitHub;
using Amaurot.Processor.SourceGenerationContext;

namespace Amaurot.Processor;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static async Task Main(string[] args)
    {
        var arguments = JsonSerializer.Deserialize(
            Encoding.Default.GetString(Convert.FromBase64String(args[0])),
            SnakeCaseLowerSourceGenerationContext.Default.Arguments
        );

        var githubClient = new GitHubClient(
            arguments.Repository,
            arguments.Number,
            arguments.HeadCommit,
            arguments.MergeCommit,
            arguments.InstallationId
        );

        var directory = await githubClient.DownloadRepositoryArchive();
    }

    internal sealed class Arguments
    {
        public required string Action { get; init; }
        public required string Repository { get; init; }
        public required long Number { get; init; }
        public required string Deployment { get; init; }
        public required string HeadCommit { get; init; }
        public required string MergeCommit { get; init; }
        public required long InstallationId { get; init; }
    }
}
