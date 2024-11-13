using System.Net;
using Anyder.EventProcessors;
using Elpis.Clients;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;

namespace Anyder;

public class Program
{
    private static readonly string? GitHubWebhookSecret = Environment.GetEnvironmentVariable(
        "GITHUB_WEBHOOK_SECRET"
    );

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
        builder.WebHost.ConfigureKestrel(
            (_, serverOptions) =>
                serverOptions.Listen(
                    IPAddress.Loopback,
                    int.Parse(Environment.GetEnvironmentVariable("PORT") ?? "5000")
                )
        );
        builder.Services.AddSingleton<WebhookEventProcessor, GitHubWebhookEventProcessor>();

        var app = builder.Build();
        app.MapGitHubWebhooks(secret: GitHubWebhookSecret);
        app.Run();
    }
}
