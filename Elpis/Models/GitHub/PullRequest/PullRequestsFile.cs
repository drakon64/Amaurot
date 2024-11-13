using System.Text.Json.Serialization;

namespace Elpis.Models.GitHub.PullRequest;

public class PullRequestsFile
{
    [JsonPropertyName("filename")]
    public required string FileName { get; init; }
}
