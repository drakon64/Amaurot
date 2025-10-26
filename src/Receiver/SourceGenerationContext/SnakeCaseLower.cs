using System.Text.Json.Serialization;

using Amaurot.Receiver.Client.GitHub;

namespace Amaurot.Receiver.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubClient.InstallationAccessToken))]
internal sealed partial class SnakeCaseLowerSourceGenerationContext : JsonSerializerContext;
