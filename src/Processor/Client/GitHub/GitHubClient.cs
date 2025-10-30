using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

using Microsoft.IdentityModel.Tokens;

namespace Amaurot.Processor.Client.GitHub;

internal partial class GitHubClient
{
    private readonly string _clientId =
        Environment.GetEnvironmentVariable("AMAUROT_GITHUB_CLIENT_ID")
        ?? throw new InvalidOperationException("AMAUROT_GITHUB_CLIENT_ID is null");

    private readonly HttpClient _httpClient;
    private readonly long _installationId;
    private readonly SigningCredentials _signingCredentials;

    internal GitHubClient(HttpClient client, long installationId)
    {
        _httpClient = client;
        _installationId = installationId;

        var rsa = RSA.Create();
        rsa.ImportFromPem(
            Environment.GetEnvironmentVariable("AMAUROT_GITHUB_PRIVATE_KEY")
                ?? throw new InvalidOperationException("AMAUROT_GITHUB_PRIVATE_KEY is null")
        );

        _signingCredentials = new SigningCredentials(
            new RsaSecurityKey(rsa),
            SecurityAlgorithms.RsaSha256
        )
        {
            CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false },
        };
    }

    private string GenerateJwt()
    {
        var now = DateTime.UtcNow;
        var expires = now.AddSeconds(60);

        var jwt = new JwtSecurityToken(
            issuer: _clientId,
            claims:
            [
                new Claim(
                    "iat",
                    new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer
                ),
            ],
            expires: expires,
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private async Task<string> GetInstallationAccessToken()
    {
        var response = await _httpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers =
                {
                    { "Authorization", $"Bearer {GenerateJwt()}" },
                    { "User-Agent", "Amaurot/0.0.1" },
                    { "Accept", "application/vnd.github+json" },
                    { "X-GitHub-Api-Version", "2022-11-28" },
                },
                Method = HttpMethod.Post,
                RequestUri = new Uri(
                    $"https://api.github.com/app/installations/{_installationId}/access_tokens"
                ),
            }
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        var token = await response.Content.ReadFromJsonAsync<InstallationAccessToken>(
            SnakeCaseLowerSourceGenerationContext.Default.InstallationAccessToken
        );

        return $"Bearer {token!.Token}";
    }

    internal sealed class InstallationAccessToken
    {
        public required string Token { get; init; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [JsonSerializable(typeof(InstallationAccessToken))]
    private sealed partial class SnakeCaseLowerSourceGenerationContext : JsonSerializerContext;
}
