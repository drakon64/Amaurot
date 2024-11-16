using System.Text.Json.Serialization;
using Amaurot.Lib.Models.GitHub;
using Amaurot.Lib.Models.GitHub.Commit;
using Amaurot.Lib.Models.GitHub.PullRequest;

namespace Amaurot.Lib.Models;

[JsonSerializable(typeof(CreateCommitStatusRequest))]
[JsonSerializable(typeof(PullRequest))]
[JsonSerializable(typeof(PullRequestFile[]))]
[JsonSerializable(typeof(InstallationAccessToken))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    UseStringEnumConverter = true
)]
internal partial class AmaurotSerializerContext : JsonSerializerContext;
