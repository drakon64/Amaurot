using Amaurot.Lib.Clients;
using Amaurot.Receiver.EventProcessors;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

namespace Amaurot.Receiver;

public class Program
{
    private static readonly string GitHubPrivateKey =
        Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
        ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

    private static readonly string GitHubClientId =
        Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
        ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

    public static readonly GitHubClient GitHubClient = new(GitHubPrivateKey, GitHubClientId);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<WebhookEventProcessor, GitHubWebhookEventProcessor>();

        var app = builder.Build();

        app.MapGitHubWebhooks(secret: Environment.GetEnvironmentVariable("GITHUB_WEBHOOK_SECRET"));
        app.MapGet("/healthcheck", () => Results.Ok());

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT") ?? "5000"}");
    }
}
