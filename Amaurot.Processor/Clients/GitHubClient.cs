using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Amaurot.Common.Models;
using Amaurot.Processor.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.GitHub.Issues;
using Amaurot.Processor.Models.GitHub.PullRequest;
using Microsoft.IdentityModel.Tokens;

namespace Amaurot.Processor.Clients;

internal class GitHubClient
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

    private async Task<string> GetGitHubInstallationAccessToken(long installationId)
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

    public async Task<AmaurotJsonRedo> GetRepositoryAmaurotJson(TaskRequestBody taskRequestBody)
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(taskRequestBody.InstallationId)}"
                    },
                },
                RequestUri = new Uri(
                    $"{GitHubApiUri}repos/{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}/contents/amaurot.json"
                ),
            }
        );

        var amaurotJson = await responseMessage.Content.ReadFromJsonAsync<RepositoryAmaurotJson>();

        return JsonSerializer.Deserialize<AmaurotJsonRedo>(
            Encoding.UTF8.GetString(Convert.FromBase64String(amaurotJson!.Content))
        );
    }

    public async Task<PullRequestFile[]> ListPullRequestFiles(TaskRequestBody taskRequestBody)
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(taskRequestBody.InstallationId)}"
                    },
                },
                RequestUri = new Uri(
                    $"{GitHubApiUri}repos/{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}/pulls/{taskRequestBody.PullRequest}/files"
                ),
            }
        );

        return (await responseMessage.Content.ReadFromJsonAsync<PullRequestFile[]>())!;
    }

    public async Task<PullRequest?> GetPullRequest(TaskRequestBody taskRequestBody)
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(taskRequestBody.InstallationId)}"
                    },
                },
                RequestUri = new Uri(
                    $"{GitHubApiUri}repos/{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}/pulls/{taskRequestBody.PullRequest}"
                ),
            }
        );

        return await responseMessage.Content.ReadFromJsonAsync<PullRequest>(
            AmaurotSerializerContext.Default.PullRequest
        );
    }

    public async Task CreateCommitStatus(
        CreateCommitStatusRequest commitStatusRequest,
        TaskRequestBody taskRequestBody
    )
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(taskRequestBody.InstallationId)}"
                    },
                },
                RequestUri = new Uri(
                    $"{GitHubApiUri}repos/{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}/statuses/{taskRequestBody.Sha}"
                ),
                Content = JsonContent.Create(
                    inputValue: commitStatusRequest,
                    jsonTypeInfo: AmaurotSerializerContext.Default.CreateCommitStatusRequest
                ),
            }
        );

        if (responseMessage.IsSuccessStatusCode)
        {
            return;
        }

        var error = await responseMessage.Content.ReadFromJsonAsync<GitHubError>(
            AmaurotSerializerContext.Default.GitHubError
        );

        throw new Exception(error!.ToString());
    }

    public async Task<Stream> DownloadRepositoryArchiveZip(
        TaskRequestBody taskRequestBody,
        string commit
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
                        $"Bearer {await GetGitHubInstallationAccessToken(taskRequestBody.InstallationId)}"
                    },
                },
                RequestUri = new Uri(
                    $"{GitHubApiUri}repos/{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}/zipball/{commit}"
                ),
            }
        );

        return await responseMessage.Content.ReadAsStreamAsync();
    }

    public async Task CreateIssueComment(string body, TaskRequestBody taskRequestBody)
    {
        var responseMessage = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    {
                        "Authorization",
                        $"Bearer {await GetGitHubInstallationAccessToken(taskRequestBody.InstallationId)}"
                    },
                },
                RequestUri = new Uri(
                    $"{GitHubApiUri}repos/{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}/issues/{taskRequestBody.PullRequest}/comments"
                ),
                Content = JsonContent.Create(
                    inputValue: new CreateIssueCommentRequest { Body = body },
                    jsonTypeInfo: AmaurotSerializerContext.Default.CreateIssueCommentRequest
                ),
            }
        );

        if (responseMessage.IsSuccessStatusCode)
        {
            return;
        }

        var error = await responseMessage.Content.ReadFromJsonAsync<GitHubError>(
            AmaurotSerializerContext.Default.GitHubError
        );

        throw new Exception(error!.ToString());
    }
}
