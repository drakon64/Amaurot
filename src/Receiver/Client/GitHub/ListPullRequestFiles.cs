using Amaurot.Receiver.SourceGenerationContext;

namespace Amaurot.Receiver.Client.GitHub;

internal partial class GitHubClient
{
    internal async Task<PullRequestFile[]> ListPullRequestFiles()
    {
        var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers =
                {
                    { "Authorization", await GetInstallationAccessToken() },
                    { "Accept", "application/vnd.github+json" },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
                RequestUri = new Uri($"https://api.github.com/repos/{repo}/pulls/{number}/files"),
            }
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception();

        var files = await response.Content.ReadFromJsonAsync<PullRequestFile[]>(
            SnakeCaseLowerSourceGenerationContext.Default.PullRequestFileArray
        );

        return files!.Length > 0 ? files : throw new Exception();
    }

    internal sealed class PullRequestFile
    {
        public required string Filename { get; init; }
    }
}
