namespace Amaurot.Processor.Models.GitHub;

internal record GitHubError
{
    public required string Message { get; init; }
    public required string DocumentationUrl { get; init; }
    public required string Status { get; init; }
}
