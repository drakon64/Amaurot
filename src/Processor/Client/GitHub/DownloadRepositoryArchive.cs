using System.IO.Compression;

namespace Amaurot.Processor.Client.GitHub;

internal sealed partial class GitHubClient
{
    internal async Task<string> DownloadRepositoryArchive()
    {
        using var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers =
                {
                    { "Accept", "application/vnd.github+json" },
                    { "Authorization", await GetInstallationAccessToken() },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
                RequestUri = new Uri($"https://api.github.com/repos/{repo}/zipball/{mergeCommit}"),
            }
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception();

        using var zipball = new ZipArchive(await response.Content.ReadAsStreamAsync());

        var directory = Directory.CreateTempSubdirectory();

        zipball.ExtractToDirectory(directory.FullName);

        return directory.FullName;
    }
}
