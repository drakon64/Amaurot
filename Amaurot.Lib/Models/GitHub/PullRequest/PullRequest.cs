namespace Amaurot.Lib.Models.GitHub.PullRequest;

public class PullRequest
{
    public required bool? Mergeable { get; init; }
    public string? MergeCommitSha { get; init; }
}
