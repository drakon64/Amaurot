using System.Text.Json.Serialization;
using Amaurot.Processor.Models.GitHub;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.GitHub.PullRequest;

namespace Amaurot.Processor.Models;

[JsonSerializable(typeof(CreateCommitStatusRequest))]
[JsonSerializable(typeof(PullRequest))]
[JsonSerializable(typeof(GitHubError))]
[JsonSerializable(typeof(InstallationAccessToken))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    UseStringEnumConverter = true
)]
internal partial class AmaurotSerializerContext : JsonSerializerContext;
