using Amaurot.Processor.Models.GitHub.Commit;

namespace Amaurot.Processor.Models.OpenTofu;

internal class ExecutionOutputs
{
    public required ExecutionOutput Init { get; init; }
    public ExecutionOutput? Plan { get; set; }
    public ExecutionOutput? Apply { get; set; }
}

internal class ExecutionOutput
{
    public required ExecutionType ExecutionType { get; init; }
    public required CommitStatusState ExecutionState { get; init; }
    public required string ExecutionStdout { get; init; }
}
