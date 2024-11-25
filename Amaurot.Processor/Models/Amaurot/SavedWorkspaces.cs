using Google.Cloud.Firestore;

namespace Amaurot.Processor.Models.Amaurot;

[FirestoreData]
internal class SavedWorkspaces
{
    [FirestoreProperty]
    public required string PullRequest { get; init; }

    [FirestoreProperty]
    public required Workspace[] Workspaces { get; init; }
}
