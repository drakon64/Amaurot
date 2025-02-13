using Amaurot.Receiver.EventProcessors;
using Google.Cloud.Tasks.V2;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

namespace Amaurot.Receiver;

public class Program
{
    internal static readonly string QueueId =
        Environment.GetEnvironmentVariable("QUEUE_ID")
        ?? throw new InvalidOperationException("QUEUE_ID is null");

    internal static readonly string ServiceAccountEmail =
        Environment.GetEnvironmentVariable("SERVICE_ACCOUNT_EMAIL")
        ?? throw new InvalidOperationException("SERVICE_ACCOUNT_EMAIL is null");

    internal static readonly string ProcessorUrl =
        Environment.GetEnvironmentVariable("PROCESSOR_URL")
        ?? throw new InvalidOperationException("PROCESSOR_URL is null");

    internal static readonly string[] AllowedRepositories = (
        Environment.GetEnvironmentVariable("ALLOWED_REPOSITORIES")
        ?? throw new InvalidOperationException("ALLOWED_REPOSITORIES is null")
    ).Split(" ");

    internal static readonly CloudTasksClient CloudTasksClient = CloudTasksClient.Create();

    public static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddSingleton<WebhookEventProcessor, GitHubWebhookEventProcessor>();

        var app = builder.Build();

        app.MapGitHubWebhooks(secret: Environment.GetEnvironmentVariable("GITHUB_WEBHOOK_SECRET"));
        app.MapGet("/", () => Results.Ok());

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
