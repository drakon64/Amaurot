using System.Text.Json;
using Amaurot.Common.Models;
using Google.Cloud.Tasks.V2;
using Google.Protobuf;
using Octokit.Webhooks;
using Octokit.Webhooks.Events;
using Octokit.Webhooks.Events.PullRequest;
using HttpMethod = Google.Cloud.Tasks.V2.HttpMethod;
using HttpRequest = Google.Cloud.Tasks.V2.HttpRequest;
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
        if (!Program.AllowedRepositories.Contains(pullRequestEvent.Repository!.FullName))
        {
            return;
        }

        string endpoint;

        if (
            (
                pullRequestAction == PullRequestAction.Opened
                || pullRequestAction == PullRequestAction.Synchronize
            ) && !pullRequestEvent.PullRequest.Draft
        )
            endpoint = "plan";
        else if (
            pullRequestAction == PullRequestAction.Closed
            && pullRequestEvent.PullRequest.Merged is true
        )
            endpoint = "apply";
        else
            return;

        await Program.CloudTasksClient.CreateTaskAsync(
            new CreateTaskRequest
            {
                Parent = Program.QueueId,
                Task = new Google.Cloud.Tasks.V2.Task
                {
                    HttpRequest = new HttpRequest
                    {
                        Body = ByteString.CopyFromUtf8(
                            JsonSerializer.Serialize(
                                new TaskRequestBody
                                {
                                    RepositoryOwner = pullRequestEvent.Repository!.Owner.Login,
                                    RepositoryName = pullRequestEvent.Repository.Name,
                                    PullRequest = pullRequestEvent.Number,
                                    Sha = pullRequestEvent.PullRequest.Head.Sha,
                                    InstallationId = pullRequestEvent.Installation!.Id,
                                },
                                TaskRequestBodyJsonSerializerContext.Default.TaskRequestBody
                            )
                        ),
                        Headers = { { "Content-Type", "application/json" } },
                        HttpMethod = HttpMethod.Post,
                        OidcToken = new OidcToken
                        {
                            ServiceAccountEmail = Program.ServiceAccountEmail,
                        },
                        Url = $"{Program.ProcessorUrl}/{endpoint}",
                    },
                },
            }
        );
    }
}
