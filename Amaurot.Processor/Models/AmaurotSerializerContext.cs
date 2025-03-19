using System.Text.Json.Serialization;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.GitHub.Issues;
using Amaurot.Processor.Models.GitHub.PullRequest;

namespace Amaurot.Processor.Models;

[JsonSerializable(typeof(CreateCommitStatusRequest))]
[JsonSerializable(typeof(CreateIssueCommentRequest))]
[JsonSerializable(typeof(PullRequest))]
[JsonSerializable(typeof(GitHubError))]
[JsonSerializable(typeof(InstallationAccessToken))]
[JsonSerializable(typeof(AmaurotJson))]
[JsonSerializable(typeof(SavedWorkspaces))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    UseStringEnumConverter = true
)]
internal partial class AmaurotSerializerContext : JsonSerializerContext;
