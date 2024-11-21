using Google.Cloud.Firestore;

namespace Amaurot.Processor.Models.Amaurot;

[FirestoreData]
internal class SavedPlan
{
    [FirestoreProperty]
    public required string PullRequest { get; init; }

    [FirestoreProperty]
    public required string Sha { get; init; }

    [FirestoreProperty]
    public required string Directory { get; init; }

    [FirestoreProperty]
    public required string Workspace { get; init; }

    [FirestoreProperty]
    public required byte[] PlanOut { get; init; }
}
