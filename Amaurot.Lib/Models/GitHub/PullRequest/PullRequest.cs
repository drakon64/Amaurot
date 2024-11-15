using System.Text.Json.Serialization;

namespace Amaurot.Lib.Models.GitHub.PullRequest;

public class PullRequest
{
    public required bool? Mergeable { get; init; }

    [JsonPropertyName("merge_commit_sha")]
    public string? MergeCommitSha { get; init; }
}
