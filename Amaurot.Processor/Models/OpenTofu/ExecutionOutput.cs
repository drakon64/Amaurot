using Amaurot.Processor.Models.GitHub.Commit;

namespace Amaurot.Processor.Models.OpenTofu;

internal class ExecutionOutput
{
    public required ExecutionType ExecutionType { get; init; }
    public required CommitStatusState ExecutionState { get; init; }
    public required string ExecutionStdout { get; init; }
}
