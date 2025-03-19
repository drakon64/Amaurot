using System.Text.Json.Serialization;

namespace Amaurot.Processor.Models.GoogleCloud;

internal class AccessTokenResponse
{
    public required string AccessToken { get; init; }
    public required ushort ExpiresIn { get; init; }
    public required string TokenType { get; init; }

    public readonly DateTime ExpiresAt;

    public AccessTokenResponse()
    {
        ExpiresAt = DateTime.Now + TimeSpan.FromSeconds(ExpiresIn);
    }
}

[JsonSerializable(typeof(AccessTokenResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal partial class AccessTokenResponseSerializerContext : JsonSerializerContext;
