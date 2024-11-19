using Amaurot.Common.Models;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor.Models.Amaurot;

internal class AmaurotComment
{
    public required TaskRequestBody TaskRequestBody { get; init; }
    public required Dictionary<
        string,
        Dictionary<string, ExecutionOutputs>
    > DirectoryOutputs { get; init; }
}
