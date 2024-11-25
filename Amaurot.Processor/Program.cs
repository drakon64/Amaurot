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

    private static readonly string GitHubContext =
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

                var changedWorkspaces = await AmaurotClient.GetWorkspaces(
                    taskRequestBody,
                    pullRequestFull
                );

                await AmaurotClient.CreateCommitStatus(
                    taskRequestBody,
                    pullRequestFull,
                    CommitStatusState.Pending,
                    GitHubContext
                );

                var tempDirectory = await AmaurotClient.ExtractPullRequestZipball(
                    taskRequestBody,
                    changedWorkspaces.MergeCommitSha
                );

                var executionState = CommitStatusState.Success;

                foreach (var workspace in changedWorkspaces.Workspaces)
                {
                    var workingDirectory =
                        $"{tempDirectory.FullName}/{taskRequestBody.RepositoryOwner}-{taskRequestBody.RepositoryName}-{changedWorkspaces.MergeCommitSha}";

                    var init = await TofuClient.TofuExecution(
                        workingDirectory,
                        workspace,
                        ExecutionType.Init
                    );

                    workspace.InitStdout = init.ExecutionStdout;

                    var plan = await TofuClient.TofuExecution(
                        workingDirectory,
                        workspace,
                        ExecutionType.Plan
                    );

                    workspace.ExecutionStdout = plan.ExecutionStdout;
                    workspace.PlanOut = plan.PlanOut;

                    if (
                        init.ExecutionState == CommitStatusState.Error
                        || plan.ExecutionState == CommitStatusState.Error
                    )
                    {
                        executionState = CommitStatusState.Error;
                    }
                }

                tempDirectory.Delete(true);

                if (executionState != CommitStatusState.Error)
                {
                    await AmaurotClient.SavePlanOutput(
                        taskRequestBody.Sha,
                        new SavedWorkspaces
                        {
                            PullRequest = pullRequestFull,
                            Workspaces = changedWorkspaces.Workspaces,
                        }
                    );
                }

                await AmaurotClient.CreateComment(
                    taskRequestBody,
                    changedWorkspaces.Workspaces,
                    "Plan"
                );

                await AmaurotClient.CreateCommitStatus(
                    taskRequestBody,
                    pullRequestFull,
                    executionState,
                    GitHubContext
                );

                return Results.Ok();
            }
        );

        app.MapPost(
            "/apply",
            async (TaskRequestBody taskRequestBody) =>
            {
                var pullRequestFull =
                    $"{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}#{taskRequestBody.PullRequest}";

                var changedWorkspaces = await AmaurotClient.GetSavedPlanOutput(taskRequestBody.Sha);

                var tempDirectory = await AmaurotClient.ExtractPullRequestZipball(
                    taskRequestBody,
                    taskRequestBody.Sha
                );

                foreach (var workspace in changedWorkspaces.Workspaces)
                {
                    var workingDirectory =
                        $"{tempDirectory.FullName}/{taskRequestBody.RepositoryOwner}-{taskRequestBody.RepositoryName}-{taskRequestBody.Sha}";

                    var init = await TofuClient.TofuExecution(
                        workingDirectory,
                        workspace,
                        ExecutionType.Init
                    );

                    workspace.InitStdout = init.ExecutionStdout;

                    var apply = await TofuClient.TofuExecution(
                        workingDirectory,
                        workspace,
                        ExecutionType.Apply
                    );

                    workspace.ExecutionStdout = apply.ExecutionStdout;
                }

                tempDirectory.Delete(true);

                await AmaurotClient.CreateComment(
                    taskRequestBody,
                    changedWorkspaces.Workspaces,
                    "Apply"
                );
            }
        );

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
