namespace Amaurot.Processor.Models.Amaurot;

internal class SavedWorkspaces
{
    public required string PullRequest { get; init; }
    public required Workspace[] Workspaces { get; init; }
}
