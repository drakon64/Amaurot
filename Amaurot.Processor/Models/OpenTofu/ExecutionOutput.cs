using Amaurot.Processor.Models.GitHub.Commit;

namespace Amaurot.Processor.Models.OpenTofu;

internal class ExecutionOutput
{
    public required CommitStatusState ExecutionState { get; init; }
    public required string ExecutionStdout { get; init; }
    public byte[]? PlanOut { get; init; }
}
