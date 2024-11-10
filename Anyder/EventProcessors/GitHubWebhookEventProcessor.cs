using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;

namespace Anyder.EventProcessors;

public sealed class GitHubWebhookEventProcessor : WebhookEventProcessor
{
    protected override async Task ProcessPullRequestWebhookAsync(
        WebhookHeaders webhookHeaders,
        PullRequestEvent pullRequestEvent,
        PullRequestAction pullRequestAction
    )
    {
        if (
            pullRequestAction == PullRequestAction.Opened
            || pullRequestAction == PullRequestAction.Synchronize
        )
        {
            await Program.GitHubClient.CreateCommitStatus(
                pullRequestEvent.Repository!.FullName,
                pullRequestEvent.PullRequest.Head.Sha,
                pullRequestEvent.Installation!.Id
            );
        }
    }
}
