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
        switch (action)
        {
            case PullRequestActionValue.Opened:
                logger.LogInformation("pull request opened");
                break;
            default:
                logger.LogInformation("Some other pull request event");
                break;
        }

        await Task.Delay(1000, cancellationToken);
    }
}
