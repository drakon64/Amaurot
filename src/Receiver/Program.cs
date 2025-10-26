using Amaurot.Receiver.Webhook;

using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

namespace Amaurot.Receiver;

internal static class Program
{
    internal static readonly HttpClient HttpClient = new();

    private static void Main()
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
