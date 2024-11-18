namespace Amaurot.Processor.Models.OpenTofu;

internal class AmaurotJson
{
    public required Dictionary<string, Workspace> Workspaces { get; init; }
}
