namespace Amaurot.Processor.Models.GitHub.Commit;

// https://github.com/dotnet/runtime/issues/92828

public enum CommitStatusState
{
    Error,
    Failure,
    Pending,
    Success,
}
