using System.Text.Json.Serialization;

using Amaurot.Receiver.Client.CloudRun;
using Amaurot.Receiver.Client.GitHub;

namespace Amaurot.Receiver.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GitHubClient.InstallationAccessToken))]
[JsonSerializable(typeof(GitHubClient.PullRequest))]
[JsonSerializable(typeof(GitHubClient.PullRequestFile[]))]
[JsonSerializable(typeof(CloudRunClient.AccessTokenResponse))]
internal sealed partial class SnakeCaseLowerSourceGenerationContext : JsonSerializerContext;
