namespace Amaurot.Receiver.Client.GitHub;

internal partial class GitHubClient
{
    internal async Task<byte[]> GetAmaurotJson(string commit)
    {
        var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers =
                {
                    { "Accept", "application/vnd.github.raw+json" },
                    { "Authorization", await GetInstallationAccessToken() },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
                RequestUri = new Uri(
                    $"https://api.github.com/repos/{repo}/contents/amaurot.json?ref={commit}"
                ),
            }
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception();

        return await response.Content.ReadAsByteArrayAsync();
    }
}
