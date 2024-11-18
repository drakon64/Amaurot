namespace Amaurot.Processor.Models.OpenTofu;

internal class Workspaces
{
    public required Dictionary<string, Workspace> Workspace { get; init; }
}

internal class Workspace
{
    public string[]? VarFiles { get; init; }
}
