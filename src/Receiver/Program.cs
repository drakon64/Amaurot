using Amaurot.Receiver.Webhook;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

namespace Amaurot.Receiver;

internal static class Program
{
    internal static readonly string Project =
        Environment.GetEnvironmentVariable("AMAUROT_PROJECT")
        ?? throw new InvalidOperationException("AMAUROT_PROJECT is null");

    internal static readonly string Region =
        Environment.GetEnvironmentVariable("AMAUROT_REGION")
        ?? throw new InvalidOperationException("AMAUROT_REGION is null");

    internal static readonly string Processor =
        Environment.GetEnvironmentVariable("AMAUROT_PROCESSOR")
        ?? throw new InvalidOperationException("AMAUROT_PROCESSOR is null");

    internal static readonly HttpClient HttpClient = new();

    internal static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.Services.AddSingleton<WebhookEventProcessor, PullRequestWebhookEventProcessor>();

        var app = builder.Build();

        app.MapGitHubWebhooks(
            secret: Environment.GetEnvironmentVariable("AMAUROT_GITHUB_WEBHOOK_SECRET")
        );

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
