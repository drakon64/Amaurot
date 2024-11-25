using Google.Cloud.Firestore;

namespace Amaurot.Processor.Models.Amaurot;

[FirestoreData]
internal class Workspace
{
    [FirestoreProperty]
    public required string Name { get; init; }

    [FirestoreProperty]
    public required string Directory { get; init; }

    [FirestoreProperty]
    public string[]? VarFiles { get; init; }
}
