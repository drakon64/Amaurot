using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Amaurot.Authentication;

internal class BearerTokenAuthenticationProvider : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = new()
    )
    {
        var apiKey =
            Environment.GetEnvironmentVariable("GITHUB_TOKEN")
            ?? throw new InvalidOperationException("GITHUB_TOKEN is null");

        request.Headers.Add("Authorization", $"Bearer {apiKey}");

        return Task.CompletedTask;
    }
}
