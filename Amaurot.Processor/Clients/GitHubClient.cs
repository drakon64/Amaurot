using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Amaurot.Processor.Models;
using Amaurot.Processor.Models.GitHub;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.GitHub.PullRequest;
using Microsoft.IdentityModel.Tokens;

namespace Amaurot.Processor.Clients;

public class GitHubClient
{
    private readonly SigningCredentials _githubSigningCredentials;
    private readonly string _githubClientId;

    public GitHubClient(string githubPrivateKey, string githubClientId)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(githubPrivateKey);

        _githubSigningCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256
        )
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
        };

        _githubClientId = githubClientId;
    }

    private string GenerateJwtSecurityToken()
    {
        var now = DateTime.UtcNow;
        var expires = now.AddSeconds(100);

        var jwt = new JwtSecurityToken(
            issuer: _githubClientId,
            claims:
            [
                new Claim(
                    "iat",
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer
                ),
            ],
            expires: expires,
            signingCredentials: _githubSigningCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static readonly HttpClient HttpClient =
        new()
        {
            DefaultRequestHeaders =
            {
                { "User-Agent", "Amaurot/0.0.1" },
                { "Accept", "application/vnd.github+json" },
                { "X-GitHub-Api-Version", "2022-11-28" },
            },
        };

    private const string GitHubApiUri = "https://api.github.com/";

    private InstallationAccessToken _githubInstallationAccessToken =
        new() { Token = "", ExpiresAt = DateTime.Now };

    private async Task<string> GetGitHubInstallationAccessToken(string installationId)
    {
        // If the current installation access token expires in less than a minute, generate a new one
        if (_githubInstallationAccessToken.ExpiresAt.Subtract(DateTime.Now).Minutes >= 1)
            return _githubInstallationAccessToken.Token;

        await Console.Out.WriteLineAsync("Generating new GitHub installation access token");

        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { { "Authorization", $"Bearer {GenerateJwtSecurityToken()}" } },
                RequestUri = new Uri(
                    $"{GitHubApiUri}app/installations/{installationId}/access_tokens"
                ),
            }
        );

        _githubInstallationAccessToken = (
            await responseMessage.Content.ReadFromJsonAsync<InstallationAccessToken>(
                AmaurotSerializerContext.Default.InstallationAccessToken
            )
        )!;

        return _githubInstallationAccessToken.Token;
    }

    public async Task<PullRequestFile[]> ListPullRequestFiles(
        string repo,
        long pullRequest,
        long installationId
    )
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(installationId.ToString())}"
                    },
                },
                RequestUri = new Uri($"{GitHubApiUri}repos/{repo}/pulls/{pullRequest}/files"),
            }
        );

        return (await responseMessage.Content.ReadFromJsonAsync<PullRequestFile[]>())!;
    }

    public async Task<PullRequest?> GetPullRequest(
        string repo,
        long pullRequest,
        long installationId
    )
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(installationId.ToString())}"
                    },
                },
                RequestUri = new Uri($"{GitHubApiUri}repos/{repo}/pulls/{pullRequest}"),
            }
        );

        return await responseMessage.Content.ReadFromJsonAsync<PullRequest>(
            AmaurotSerializerContext.Default.PullRequest
        );
    }

    public async Task CreateCommitStatus(
        string repo,
        string sha,
        CommitStatusState state,
        string context,
        long installationId
    )
    {
        await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(installationId.ToString())}"
                    },
                },
                RequestUri = new Uri($"{GitHubApiUri}repos/{repo}/statuses/{sha}"),
                Content = JsonContent.Create(
                    inputValue: new CreateCommitStatusRequest { State = state, Context = context },
                    jsonTypeInfo: AmaurotSerializerContext.Default.CreateCommitStatusRequest
                ),
            }
        );
    }

    public async Task<Stream> DownloadRepositoryArchiveZip(
        string repo,
        string sha,
        long installationId
    )
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(installationId.ToString())}"
                    },
                },
                RequestUri = new Uri($"{GitHubApiUri}repos/{repo}/zipball/{sha}"),
            }
        );

        return await responseMessage.Content.ReadAsStreamAsync();
    }
}
