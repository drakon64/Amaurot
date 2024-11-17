namespace Amaurot.Processor.Models.GitHub;

public class InstallationAccessToken
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
