using Amaurot.Receiver.SourceGenerationContext;

namespace Amaurot.Receiver.Client;

internal static class GoogleCloudClient
{
    internal static async Task<string> GetAccessToken()
    {
        var response = await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers = { { "Metadata-Flavor", "Google" } },
                Method = HttpMethod.Get,
                RequestUri = new Uri(
                    "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/token"
                ),
            }
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception(await response.Content.ReadAsStringAsync());

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(
            SnakeCaseLowerSourceGenerationContext.Default.AccessTokenResponse
        );

        return $"{token!.TokenType} {token.AccessToken}";
    }

    internal sealed class AccessTokenResponse
    {
        public required string AccessToken { get; init; }
        public required string TokenType { get; init; }
    }
}
