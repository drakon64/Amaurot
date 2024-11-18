namespace Amaurot.Processor.Models.GitHub.Commit;

// TODO: https://github.com/dotnet/runtime/issues/92828

internal enum CommitStatusState
{
    Error,
    Failure,
    Pending,
    Success,
}
