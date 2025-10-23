using Amaurot.Receiver.Clients;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;

namespace Amaurot.Receiver.Webhook;

public class PullRequestWebhookEventProcessor(ILogger<PullRequestWebhookEventProcessor> logger)
    : WebhookEventProcessor
{
    protected override async ValueTask ProcessPullRequestWebhookAsync(
        WebhookHeaders headers,
        PullRequestEvent pullRequestEvent,
        PullRequestAction action,
        CancellationToken cancellationToken = default
    )
    {
        await Program.HttpClient.SendAsync(
            new HttpRequestMessage
            {
                Headers = { { "Authorization", await GoogleCloudClient.GetAccessToken() } },
                RequestUri = new Uri(
                    $"https://run.googleapis.com/v2/projects/{Program.Project}/locations/{Program.Region}/jobs/{Program.Processor}:run"
                ),
            },
            cancellationToken
        );
    }
}
