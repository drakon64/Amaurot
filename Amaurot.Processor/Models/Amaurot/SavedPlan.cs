namespace Amaurot.Processor.Models.Amaurot;

internal class SavedPlan
{
    public required string PullRequest { get; init; }
    public required string Sha { get; init; }
    public required string Directory { get; init; }
    public required string Workspace { get; init; }
    public required byte[] PlanOut { get; init; }
}
