using System.IO.Compression;

namespace Amaurot.Processor.Client.GitHub;

internal partial class GitHubClient
{
    internal async Task<GZipStream> DownloadRepositoryArchive()
    {
        var request = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                RequestUri = new Uri($"https://api.github.com/repos/{repository}/tarball/{commit}"),
                Headers =
                {
                    { "Accept", "application/vnd.github+json" },
                    { "Authorization", $"Bearer {await GetInstallationAccessToken()}" },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
            }
        );

        if (!request.IsSuccessStatusCode)
            throw new Exception();

        return new GZipStream(
            await request.Content.ReadAsStreamAsync(),
            CompressionMode.Decompress
        );
    }
}
