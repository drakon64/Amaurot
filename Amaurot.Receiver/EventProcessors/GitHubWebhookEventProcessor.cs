using Amaurot.Lib.Models.GitHub.Commit;
using Google.Cloud.Run.V2;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;
using Task = System.Threading.Tasks.Task;

namespace Amaurot.Receiver.EventProcessors;

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
            await Console.Out.WriteLineAsync(
                $"Pull request {pullRequestEvent.Repository.FullName}#{pullRequestEvent.Number} contains no OpenTofu configuration files"
            );

            return;
        }

        await Console.Out.WriteLineAsync(
            $"Getting mergeability of pull request {pullRequestEvent.Repository.FullName}#{pullRequestEvent.Number}"
        );

        string? mergeCommitSha;

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

            mergeCommitSha = pullRequest.MergeCommitSha;
            break;
        }

        if (mergeCommitSha is null)
        {
            await Console.Out.WriteLineAsync(
                $"Pull request {pullRequestEvent.Repository.FullName}#{pullRequestEvent.Number} is not mergeable"
            );

            return;
        }

        await Console.Out.WriteLineAsync(
            $"Creating commit status for pull request {pullRequestEvent.Repository.FullName}#{pullRequestEvent.Number} commit {pullRequestEvent.PullRequest.Head.Sha}"
        );

        await Program.GitHubClient.CreateCommitStatus(
            pullRequestEvent.Repository.FullName,
            pullRequestEvent.PullRequest.Head.Sha,
            CommitStatusState.Pending,
            "Amaurot",
            pullRequestEvent.Installation.Id
        );

        await Console.Out.WriteLineAsync(
            $"Running OpenTofu plan for pull request {pullRequestEvent.Repository.FullName}#{pullRequestEvent.Number} commit {pullRequestEvent.PullRequest.Head.Sha}"
        );

        await Program.CloudRunClient.RunJobAsync(
            new RunJobRequest
            {
                Name = "projects/drakon64-akadaemia-anyder/locations/europe-west2/jobs/ktisis",
                Overrides = new RunJobRequest.Types.Overrides
                {
                    ContainerOverrides =
                    {
                        new RunJobRequest.Types.Overrides.Types.ContainerOverride
                        {
                            Args = { tfDirectories },
                            Env =
                            {
                                new EnvVar
                                {
                                    Name = "GITHUB_REPOSITORY_OWNER",
                                    Value = pullRequestEvent.Repository.Owner.Login,
                                },
                                new EnvVar
                                {
                                    Name = "GITHUB_REPOSITORY_NAME",
                                    Value = pullRequestEvent.Repository.Name,
                                },
                                new EnvVar { Name = "GITHUB_REF", Value = mergeCommitSha },
                                new EnvVar
                                {
                                    Name = "GITHUB_INSTALLATION_ID",
                                    Value = pullRequestEvent.Installation!.Id.ToString(),
                                },
                            },
                        },
                    },
                },
            }
        );
    }
}
