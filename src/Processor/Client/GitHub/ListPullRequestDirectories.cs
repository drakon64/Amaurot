using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Amaurot.Processor.SourceGenerationContext;

namespace Amaurot.Processor.Client.GitHub;

internal partial class GitHubClient
{
    [GeneratedRegex("^.*\\.(?:tfvars|tftpl|tf)$")]
    private static partial Regex ExtensionRegex();

    internal async Task<string[]> ListPullRequestDirectories()
    {
        var request = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers =
                {
                    { "Accept", "application/vnd.github+json" },
                    { "Authorization", $"Bearer {await GetInstallationAccessToken()}" },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
                RequestUri = new Uri(
                    $"https://api.github.com/repos/{repository}/pulls/{pullRequest}/files"
                ),
            }
        );

        if (!request.IsSuccessStatusCode)
            throw new Exception();

        var files = await request.Content.ReadFromJsonAsync<PullRequestFile[]>(
            SnakeCaseLowerSourceGenerationContext.Default.PullRequestFileArray
        );

        return (
            from file in files
            where ExtensionRegex().IsMatch(file.FileName)
            select file.FileName[..file.FileName.LastIndexOf('/')]
        )
            .Distinct()
            .ToArray();
    }

    internal class PullRequestFile
    {
        [JsonPropertyName("filename")]
        public required string FileName { get; init; }
    }
}
