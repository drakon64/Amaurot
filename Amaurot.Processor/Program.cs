using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;
using Google.Cloud.Firestore;

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

    private static readonly FirestoreDb FirestoreDatabase = FirestoreDb.Create();

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

                await Console.Out.WriteLineAsync(
                    $"Getting mergeability of pull request {pullRequestFull}"
                );

                string? mergeCommitSha;

                while (true)
                {
                    var pullRequest = (await GitHubClient.GetPullRequest(taskRequestBody))!;

                    if (!pullRequest.Mergeable.HasValue)
                    {
                        await Task.Delay(3000);
                        continue;
                    }

                    mergeCommitSha = pullRequest.MergeCommitSha;
                    break;
                }

                if (mergeCommitSha is null)
                {
                    throw new Exception($"Pull request {pullRequestFull} is not mergeable");
                }

                await Console.Out.WriteLineAsync(
                    $"Getting changed directories in pull request {pullRequestFull}"
                );

                var pullRequestFiles = await GitHubClient.ListPullRequestFiles(taskRequestBody);

                var changedDirectories = (
                    from file in pullRequestFiles
                    let lastIndex = file.FileName.LastIndexOf('/')
                    select lastIndex != -1 ? file.FileName.Remove(lastIndex) : ""
                )
                    .Distinct()
                    .ToArray();

                if (changedDirectories.Length == 0)
                {
                    throw new Exception($"Pull request {pullRequestFull} is empty");
                }

                var changedTfVars = (
                    from file in pullRequestFiles
                    where
                        file.FileName.EndsWith(".tfvars") || file.FileName.EndsWith(".tfvars.json")
                    select file.FileName
                ).ToArray();

                await Console.Out.WriteLineAsync(
                    $"Getting changed workspaces in pull request {pullRequestFull}"
                );

                var amaurotJson = await GitHubClient.GetRepositoryAmaurotJson(
                    taskRequestBody,
                    mergeCommitSha
                );

                var workspaces = (
                    from changedDirectory in changedDirectories
                    from changedTfVar in changedTfVars
                    from workspace in amaurotJson.Workspaces
                    where
                        workspace.Directory == changedDirectory
                        || (
                            workspace.VarFiles is not null
                            && workspace.VarFiles.Contains(changedTfVar)
                        )
                    select workspace
                )
                    .Distinct()
                    .ToArray();

                if (workspaces.Length == 0)
                {
                    throw new Exception(
                        $"Pull request {pullRequestFull} contains no modified workspaces"
                    );
                }

                var changedWorkspaces = new ChangedWorkspaces
                {
                    Workspaces = workspaces,
                    MergeCommitSha = mergeCommitSha,
                };

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
                    await FirestoreDatabase
                        .Collection("plans")
                        .Document(taskRequestBody.Sha)
                        .SetAsync(
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

                var changedWorkspaces = (
                    await FirestoreDatabase
                        .Collection("plans")
                        .Document(taskRequestBody.Sha)
                        .GetSnapshotAsync()
                ).ConvertTo<SavedWorkspaces>();

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

                var savedPlans = await FirestoreDatabase
                    .Collection("plans")
                    .WhereEqualTo("PullRequest", pullRequestFull)
                    .GetSnapshotAsync();

                foreach (var savedPlan in savedPlans.Documents)
                {
                    await savedPlan.Reference.DeleteAsync();
                }

                await AmaurotClient.CreateComment(
                    taskRequestBody,
                    changedWorkspaces.Workspaces,
                    "Apply"
                );

                return Results.Ok();
            }
        );

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
