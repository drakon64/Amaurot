using Google.Cloud.Firestore;

namespace Amaurot.Processor.Models.Amaurot;

[FirestoreData]
internal class SavedWorkspaces
{
    [FirestoreProperty]
    public required string PullRequest { get; init; }

    [FirestoreProperty]
    public required SavedWorkspace[] Workspaces { get; init; }
}

[FirestoreData]
internal class SavedWorkspace : Workspace
{
    [FirestoreProperty]
    public required byte[] PlanOut { get; init; }
}
