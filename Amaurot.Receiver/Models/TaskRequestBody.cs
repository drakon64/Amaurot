namespace Amaurot.Receiver.Models;

internal class TaskRequestBody
{
    public required string RepositoryOwner { get; init; }
    public required string RepositoryName { get; init; }
    public required long PullRequest { get; init; }
    public required long InstallationId { get; init; }
}
