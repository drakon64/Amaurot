namespace Amaurot.Receiver.Models;

internal class TaskRequestBody
{
    internal required string RepositoryOwner { get; init; }
    internal required string RepositoryName { get; init; }
    internal required long PullRequest { get; init; }
    internal required long InstallationId { get; init; }
}
