using System.Text.Json.Serialization;
using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubClient.InstallationAccessToken))]
internal sealed partial class SnakeCaseLowerSourceGenerationContext : JsonSerializerContext;
