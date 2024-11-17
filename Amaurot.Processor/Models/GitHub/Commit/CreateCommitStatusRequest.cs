namespace Amaurot.Processor.Models.GitHub.Commit;

public class CreateCommitStatusRequest
{
    public required string State { get; init; } // TODO: https://github.com/dotnet/runtime/issues/92828
    public string? TargetUrl { get; init; }
    public string? Description { get; init; }
    public required string Context { get; init; }
}
