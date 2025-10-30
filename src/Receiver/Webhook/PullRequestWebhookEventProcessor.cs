using Amaurot.Receiver.Client.CloudRun;
using Amaurot.Receiver.Client.GitHub;

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

    protected override async ValueTask ProcessPullRequestWebhookAsync(
        WebhookHeaders headers,
        PullRequestEvent pullRequestEvent,
        PullRequestAction action,
        CancellationToken cancellationToken = default
    )
    {
        if (!Actions.Contains(action) || pullRequestEvent.PullRequest.Draft)
        {
            logger.LogInformation(
                "Not responding to {PullRequestAction} event from {FullName} #{Number}",
                action,
                pullRequestEvent.Repository!.FullName,
                pullRequestEvent.Number
            );

            return;
        }

        logger.LogInformation(
            "Responding to {PullRequestAction} event from {FullName} #{Number}",
            action,
            pullRequestEvent.Repository!.FullName,
            pullRequestEvent.Number
        );

        var commit = await new GitHubClient(
            pullRequestEvent.Repository.FullName,
            pullRequestEvent.Number,
            pullRequestEvent.Installation!.Id
        ).GetMergeCommitSha();

        await CloudRunClient.RunJob(
            pullRequestEvent.Repository.FullName,
            pullRequestEvent.Number,
            commit,
            pullRequestEvent.Installation.Id
        );
    }
}
