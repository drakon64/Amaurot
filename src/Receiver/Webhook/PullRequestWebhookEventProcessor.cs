using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;

namespace Amaurot.Receiver.Webhook;

internal sealed class PullRequestWebhookEventProcessor(
    ILogger<PullRequestWebhookEventProcessor> logger
) : WebhookEventProcessor
{
    private static readonly PullRequestAction[] Actions =
    [
        PullRequestAction.Opened,
        PullRequestAction.Reopened,
        PullRequestAction.ReadyForReview,
    ];

    protected override ValueTask ProcessPullRequestWebhookAsync(
        WebhookHeaders headers,
        PullRequestEvent pullRequestEvent,
        PullRequestAction action,
        CancellationToken cancellationToken = default
    )
    {
        if (!Actions.Contains(action))
        {
            logger.LogInformation(
                "Not responding to {PullRequestAction} event from {FullName} #{PullRequestNumber}",
                action,
                pullRequestEvent.Repository!.FullName,
                pullRequestEvent.PullRequest.Number
            );

            return new ValueTask();
        }

        logger.LogInformation(
            "Responding to {PullRequestAction} event from {FullName} #{PullRequestNumber}",
            action,
            pullRequestEvent.Repository!.FullName,
            pullRequestEvent.PullRequest.Number
        );

        return new ValueTask();
    }
}
