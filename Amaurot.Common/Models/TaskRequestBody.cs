using System.Text.Json.Serialization;

namespace Amaurot.Common.Models;

public class TaskRequestBody
{
    public required string RepositoryOwner { get; init; }
    public required string RepositoryName { get; init; }
    public required long PullRequest { get; init; }
    public required string Sha { get; init; }
    public required long InstallationId { get; init; }
}

[JsonSerializable(typeof(TaskRequestBody))]
public partial class TaskRequestBodyJsonSerializerContext : JsonSerializerContext;
