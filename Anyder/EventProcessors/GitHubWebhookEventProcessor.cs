using Elpis.Models.GitHub.Commit;
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
            !(
                pullRequestAction == PullRequestAction.Opened
                || pullRequestAction == PullRequestAction.Synchronize
            ) && !pullRequestEvent.PullRequest.Draft
        )
        {
            return;
        }

        var tfDirectories = (
            from file in await Program.GitHubClient.ListPullRequestFiles(
                pullRequestEvent.Repository!.FullName,
                pullRequestEvent.Number,
                pullRequestEvent.Installation!.Id
            )
            where
                file.FileName.EndsWith(".tf")
                || file.FileName.EndsWith(".tf.json")
                || file.FileName.EndsWith(".tfvars")
                || file.FileName.EndsWith(".tfvars.json")
            let lastIndex = file.FileName.LastIndexOf('/')
            select lastIndex != -1 ? file.FileName.Remove(lastIndex) : ""
        )
            .Distinct()
            .ToArray();

        if (tfDirectories.Length == 0)
        {
            return;
        }

        bool mergeable;

        while (true)
        {
            var pullRequest = await Program.GitHubClient.GetPullRequest(
                pullRequestEvent.Repository!.FullName,
                pullRequestEvent.Number,
                pullRequestEvent.Installation!.Id
            );

            if (!pullRequest!.Mergeable.HasValue)
            {
                await Task.Delay(3000);
                continue;
            }

            mergeable = pullRequest.Mergeable.Value;
            break;
        }

        if (!mergeable)
        {
            return;
        }

        await Program.GitHubClient.CreateCommitStatus(
            pullRequestEvent.Repository.FullName,
            pullRequestEvent.PullRequest.Head.Sha,
            CommitStatusState.Pending,
            "Amaurot",
            pullRequestEvent.Installation.Id
        );
    }
}
