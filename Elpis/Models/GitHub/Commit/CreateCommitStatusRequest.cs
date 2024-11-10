namespace Elpis.Models.GitHub.Commit;

public class CreateCommitStatusRequest
{
    public required CreateCommitStatusState State { get; init; }
    public string? TargetUrl { get; init; }
    public string? Description { get; init; }
    public required string Context { get; init; }
}
