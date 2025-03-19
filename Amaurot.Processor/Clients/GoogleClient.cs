using System.Text;
using System.Text.Json;
using Amaurot.Processor.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GoogleCloud;
using Microsoft.AspNetCore.WebUtilities;

namespace Amaurot.Processor.Clients;

internal static class GoogleClient
{
    private static readonly HttpClient HttpClient = new();

    private static AccessTokenResponse _accessToken = new()
    {
        AccessToken = "",
        ExpiresIn = 0,
        TokenType = "",
    };

    private static async Task<AccessTokenResponse> GetAccessToken()
    {
        // If the current installation access token expires in less than a minute, generate a new one
        if (_accessToken.ExpiresAt.Subtract(DateTime.Now).Minutes >= 1)
            return _accessToken;

        var request = await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Headers = { { "Metadata-Flavor", "Google" } },
                RequestUri = new Uri(
                    "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token"
                ),
            }
        );

        _accessToken = (
            await request.Content.ReadFromJsonAsync<AccessTokenResponse>(
                AccessTokenResponseSerializerContext.Default.AccessTokenResponse
            )
        )!;

        return _accessToken;
    }

    public static async Task SavePlanOutput(
        string bucket,
        string name,
        SavedWorkspaces savedWorkspaces
    )
    {
        var accessToken = await GetAccessToken();

        var url = $"https://storage.googleapis.com/upload/storage/v1/b/{bucket}/o";
        var parameters = new Dictionary<string, string>
        {
            { "name", name },
            { "uploadType", "media" },
        };

        var body = JsonSerializer.Serialize(
            savedWorkspaces,
            AmaurotSerializerContext.Default.SavedWorkspaces
        );

        await HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Authorization", $"{accessToken.TokenType} {accessToken.AccessToken}" },
                    { "Content-Length", body.Length.ToString() },
                    { "Content-Type", "application/json" },
                },
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes(body)),
                RequestUri = new Uri(QueryHelpers.AddQueryString(url, parameters!)),
            }
        );
    }
}
