using System.Text.Json;
using Amaurot.Receiver.Models;
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
        if (
            !(
                pullRequestAction == PullRequestAction.Opened
                || pullRequestAction == PullRequestAction.Synchronize
            ) || pullRequestEvent.PullRequest.Draft
        )
        {
            return;
        }

        await Program.CloudTasksClient.CreateTaskAsync(
            new CreateTaskRequest
            {
                Parent = Program.QueueName,
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
                                    InstallationId = pullRequestEvent.Installation!.Id,
                                },
                                AmaurotJsonSerializerContext.Default.TaskRequestBody
                            )
                        ),
                        HttpMethod = HttpMethod.Post,
                        OidcToken = new OidcToken
                        {
                            ServiceAccountEmail = Program.ServiceAccountEmail,
                        },
                        Url = Program.ProcessorUrl,
                    },
                },
            }
        );
    }
}
