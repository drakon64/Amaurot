using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor;

public class Program
{
    private static readonly string GitHubPrivateKey =
        Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
        ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

    private static readonly string GitHubClientId =
        Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
        ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

    public static readonly string GitHubContext =
        Environment.GetEnvironmentVariable("GITHUB_CONTEXT") ?? "Amaurot";

    internal static readonly GitHubClient GitHubClient = new(GitHubPrivateKey, GitHubClientId);

    public static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();

        app.MapGet("/", () => Results.Ok());

        app.MapPost(
            "/plan",
            async (TaskRequestBody taskRequestBody) =>
            {
                var pullRequestFull =
                    $"{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}#{taskRequestBody.PullRequest}";

                var workspaces = await AmaurotClient.GetWorkspaces(
                    taskRequestBody,
                    pullRequestFull
                );

                await AmaurotClient.CreateCommitStatus(
                    taskRequestBody,
                    pullRequestFull,
                    CommitStatusState.Pending
                );

                var tempDirectory = await AmaurotClient.ExtractPullRequestZipball(
                    taskRequestBody,
                    taskRequestBody.Sha
                );

                var planOutputs = new Dictionary<string, Dictionary<string, ExecutionOutputs>>();

                foreach (var workspace in workspaces.Workspaces)
                {
                    var init = await TofuClient.TofuExecution(
                        new Execution { Workspace = workspace, ExecutionType = ExecutionType.Init }
                    );

                    var plan = await TofuClient.TofuExecution(
                        new Execution { Workspace = workspace, ExecutionType = ExecutionType.Plan }
                    );

                    planOutputs[workspace.Directory][workspace.Name] = new ExecutionOutputs
                    {
                        Init = init,
                        Execution = plan,
                    };
                }

                return Results.Ok();
            }
        );

        app.MapPost(
            "/apply",
            async (TaskRequestBody taskRequestBody) =>
            {
                var savedPlan = await AmaurotClient.GetSavedPlanOutput(
                    new SavedPlanQuery
                    {
                        PullRequest =
                            $"{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}#{taskRequestBody.PullRequest}",
                        Sha = taskRequestBody.Sha,
                    }
                );
            }
        );

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
