using Amaurot.Processor.Models.Amaurot;

namespace Amaurot.Processor.Models.OpenTofu;

internal class Execution
{
    public required ExecutionType ExecutionType { get; init; }
    public required Workspace Workspace { get; init; }
}
