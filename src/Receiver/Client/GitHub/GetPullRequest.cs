using System.Diagnostics.CodeAnalysis;

using Amaurot.Receiver.SourceGenerationContext;

namespace Amaurot.Receiver.Client.GitHub;

internal partial class GitHubClient
{
    internal async Task<PullRequest> GetPullRequest()
    {
        var pullRequest = await Loop();

        while (pullRequest.Mergeable == null)
        {
            await Task.Delay(1000);
            pullRequest = await Loop();
        }

        return pullRequest;

        async Task<PullRequest> Loop()
        {
            using var response = await Program.HttpClient.SendAsync(
                new HttpRequestMessage
                {
                    Headers =
                    {
                        { "Authorization", $"{GetInstallationAccessToken()}" },
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
        public required PullRequestHead Head { get; init; }
        public bool? Mergeable { get; init; }

        [NotNull]
        public string? MergeCommitSha { get; init; }

        internal class PullRequestHead
        {
            public required string Sha { get; init; }
        }
    }
}
