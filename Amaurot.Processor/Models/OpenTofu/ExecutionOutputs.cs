using Amaurot.Processor.Models.GitHub.Commit;

namespace Amaurot.Processor.Models.OpenTofu;

internal class ExecutionOutputs
{
    public required ExecutionOutput Init { get; init; }
    public ExecutionOutput? Execution { get; init; }
}

internal class ExecutionOutput
{
    public required ExecutionType ExecutionType { get; init; }
    public required CommitStatusState ExecutionState { get; init; }
    public required string ExecutionStdout { get; init; }
    public byte[]? PlanOut { get; init; }
}
