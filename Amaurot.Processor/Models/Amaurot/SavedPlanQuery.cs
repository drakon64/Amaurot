using Google.Cloud.Firestore;

namespace Amaurot.Processor.Models.Amaurot;

[FirestoreData]
internal class SavedPlanQuery
{
    [FirestoreProperty]
    public required string PullRequest { get; init; }

    [FirestoreProperty]
    public required string Sha { get; init; }
}
