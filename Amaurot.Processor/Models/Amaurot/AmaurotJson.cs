namespace Amaurot.Processor.Models.Amaurot;

internal class AmaurotJson
{
    public required Dictionary<string, Workspace> Workspaces { get; init; }
}

internal class AmaurotJsonRedo
{
    public required AmaurotWorkspaceRedo[] Workspaces { get; init; }
}

internal class AmaurotWorkspaceRedo
{
    public required string Name { get; init; }
    public required string Directory { get; init; }
    public required string[] VarsFiles { get; init; }
}

internal class ChangedWorkspaces
{
    public required AmaurotWorkspaceRedo[] Workspaces { get; init; }
    public required string MergeCommitSha { get; init; }
}
