using Amaurot.Receiver.SourceGenerationContext;

namespace Amaurot.Receiver.Client.GitHub;

internal static partial class GitHubClient
{
    internal static async Task<string> GetMergeCommitSha(
        string repo,
        long number,
        long installationId
    )
    {
        var pullRequest = await Loop(repo, number, installationId);

        while (pullRequest.Mergeable == null)
        {
            await Task.Delay(1000);
            pullRequest = await Loop(repo, number, installationId);
        }

        return pullRequest.Mergeable == true ? pullRequest.MergeCommitSha! : throw new Exception();

        static async Task<PullRequest> Loop(string repo, long number, long installationId)
        {
            var response = await Program.HttpClient.SendAsync(
                new HttpRequestMessage
                {
                    Headers =
                    {
                        { "Authorization", $"{GetInstallationAccessToken(installationId)}" },
                        { "Accept", "application/vnd.github+json" },
                        { "X-GitHub-Api-Version", "2022-11-28" },
                    },
                    RequestUri = new Uri($"https://api.github.com/repos/{repo}/pulls/{number}"),
                }
            );

            if (!response.IsSuccessStatusCode)
                throw new Exception();

            return (
                await response.Content.ReadFromJsonAsync<PullRequest>(
                    SnakeCaseLowerSourceGenerationContext.Default.PullRequest
                )
            )!;
        }
    }

    internal sealed class PullRequest
    {
        public bool? Mergeable { get; init; }
        public string? MergeCommitSha { get; init; }
    }
}
