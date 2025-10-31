using Amaurot.Receiver.Client.Amaurot;
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

        var githubClient = new GitHubClient(
            pullRequestEvent.Repository.FullName,
            pullRequestEvent.Number,
            pullRequestEvent.Installation!.Id
        );

        var pullRequest = await githubClient.GetPullRequest();

        if (pullRequest.Mergeable == false)
        {
            logger.LogError(
                "Pull request {FullName} #{Number} is not mergeable",
                pullRequestEvent.Repository!.FullName,
                pullRequestEvent.Number
            );

            return;
        }

        await CloudRunClient.RunJob(
            pullRequestEvent.Repository.FullName,
            pullRequestEvent.Number,
            new AmaurotClient(
                await githubClient.GetRepositoryContent("amaurot.json", pullRequest.MergeCommitSha)
            ).Deployments,
            pullRequest.Head.Sha,
            pullRequest.MergeCommitSha,
            pullRequestEvent.Installation.Id
        );
    }
}
