using Anyder.Components;
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

    private static readonly string GitHubPrivateKeyPath =
        Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY_PATH")
        ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY_PATH is null");

    private static readonly string GitHubClientId =
        Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
        ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

    public static readonly GitHubClient GitHubClient = new(GitHubPrivateKeyPath, GitHubClientId);

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddSingleton<WebhookEventProcessor, GitHubWebhookEventProcessor>();
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapGitHubWebhooks(secret: GitHubWebhookSecret);
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        app.Run();
    }
}
