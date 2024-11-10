using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Elpis.Models.GitHub.Commit;
using Microsoft.IdentityModel.Tokens;

namespace Elpis.Clients;

public class GitHubClient
{
    private readonly SigningCredentials _githubSigningCredentials;
    private readonly string _githubClientId;

    public GitHubClient(string githubPrivateKeyPath, string githubClientId)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(githubPrivateKeyPath));

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
        var expires = now.AddMinutes(5);

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
            BaseAddress = new Uri("https://api.github.com/"),
            DefaultRequestHeaders =
            {
                { "User-Agent", "Amaurot/0.0.1" },
                { "Accept", "application/vnd.github+json" },
                { "X-GitHub-Api-Version", "2022-11-28" },
            },
        };

    private async Task<string> GenerateGitHubInstallationAccessToken(string installationId)
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers = { { "Authorization", $"Bearer {GenerateJwtSecurityToken()}" } },
                RequestUri = new Uri($"app/installations/{installationId}/access_tokens"),
            }
        );

        return await responseMessage.Content.ReadAsStringAsync();
    }

    public async Task CreateCommitStatus(string repo, string sha, long installationId)
    {
        await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GenerateGitHubInstallationAccessToken(installationId.ToString())}"
                    },
                },
                RequestUri = new Uri($"repos/{repo}/statuses/{sha}"),
                Content = JsonContent.Create(
                    inputValue: new CreateCommitStatusRequest
                    {
                        State = CreateCommitStatusState.Pending,
                        Context = "amaurot",
                    },
                    options: new JsonSerializerOptions
                    {
                        Converters =
                        {
                            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
                        },
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    }
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
                        $"Bearer {await GenerateGitHubInstallationAccessToken(installationId.ToString())}"
                    },
                },
                RequestUri = new Uri($"/repos/{repo}/zipball/{sha}"),
            }
        );

        return await responseMessage.Content.ReadAsStreamAsync();
    }
}
