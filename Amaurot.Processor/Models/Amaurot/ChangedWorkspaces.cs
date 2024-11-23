namespace Amaurot.Processor.Models.Amaurot;

internal class ChangedWorkspaces
{
    public required Workspace[] Workspaces { get; init; }
    public required string MergeCommitSha { get; init; }
}
