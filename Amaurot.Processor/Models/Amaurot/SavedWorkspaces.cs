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
internal class SavedWorkspace
{
    [FirestoreProperty]
    public required string Name { get; init; }

    [FirestoreProperty]
    public required string Directory { get; init; }

    [FirestoreProperty]
    public string[]? VarFiles { get; init; }

    [FirestoreProperty]
    public required byte[] PlanOut { get; init; }
}
