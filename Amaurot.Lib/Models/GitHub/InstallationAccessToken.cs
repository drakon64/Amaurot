using System.Text.Json.Serialization;

namespace Amaurot.Lib.Models.GitHub;

public class InstallationAccessToken
{
    public required string Token { get; init; }

    [JsonPropertyName("expires_at")]
    public required DateTime ExpiresAt { get; init; }
}
