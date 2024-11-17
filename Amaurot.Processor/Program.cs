using System.IO.Compression;
using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.GitHub.PullRequest;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor;

public class Program
{
    private static readonly string GithubPrivateKey =
        Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
        ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

    private static readonly string GithubClientId =
        Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
        ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

    private static readonly GitHubClient GitHubClient = new(GithubPrivateKey, GithubClientId);

    public static void Main()
    {
        var builder = WebApplication.CreateSlimBuilder();
        var app = builder.Build();

        app.MapGet("/", () => Results.Ok());

        app.MapPost(
            "/plan",
            async (TaskRequestBody taskRequestBody) =>
            {
                var repositoryFullName =
                    $"{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}";

                var tfDirectories = (
                    from file in await GitHubClient.ListPullRequestFiles(
                        repositoryFullName,
                        taskRequestBody.PullRequest,
                        taskRequestBody.InstallationId
                    )
                    where
                        file.FileName.EndsWith(".tf")
                        || file.FileName.EndsWith(".tf.json")
                        || file.FileName.EndsWith(".tfvars")
                        || file.FileName.EndsWith(".tfvars.json")
                    let lastIndex = file.FileName.LastIndexOf('/')
                    select lastIndex != -1 ? $"/{file.FileName.Remove(lastIndex)}" : "/"
                )
                    .Distinct()
                    .ToArray();

                if (tfDirectories.Length == 0)
                {
                    var result =
                        $"Pull request {repositoryFullName}#{taskRequestBody.PullRequest} contains no OpenTofu configuration files";

                    await Console.Out.WriteLineAsync(result);
                    return Results.Ok(result);
                }

                await Console.Out.WriteLineAsync(
                    $"Getting mergeability of pull request {repositoryFullName}#{taskRequestBody.PullRequest}"
                );

                PullRequest pullRequest;
                string? mergeCommitSha;

                while (true)
                {
                    pullRequest = (
                        await GitHubClient.GetPullRequest(
                            repositoryFullName,
                            taskRequestBody.PullRequest,
                            taskRequestBody.InstallationId
                        )
                    )!;

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
                    var result =
                        $"Pull request {repositoryFullName}#{taskRequestBody.PullRequest} is not mergeable";

                    await Console.Out.WriteLineAsync(result);

                    return Results.Ok(result);
                }

                await Console.Out.WriteLineAsync(
                    $"Creating commit status for pull request {repositoryFullName}#{taskRequestBody.PullRequest} commit {taskRequestBody.Sha}"
                );

                await GitHubClient.CreateCommitStatus(
                    repositoryFullName,
                    taskRequestBody.Sha,
                    // CommitStatusState.Pending,
                    "pending",
                    "Amaurot",
                    taskRequestBody.InstallationId
                );

                var zipball = await GitHubClient.DownloadRepositoryArchiveZip(
                    repositoryFullName,
                    taskRequestBody.Sha,
                    taskRequestBody.InstallationId
                );

                var tempDirectory = Directory.CreateTempSubdirectory();

                ZipFile.ExtractToDirectory(zipball, tempDirectory.FullName);

                var executionOutputs = new Dictionary<string, List<PlanOutput>>();
                var executionState = CommitStatusState.Success;

                foreach (var tfDirectory in tfDirectories)
                {
                    var directory =
                        $"{tempDirectory.FullName}/{taskRequestBody.RepositoryOwner}-{taskRequestBody.RepositoryName}-{taskRequestBody.Sha}/{tfDirectory}";

                    var init = await TofuClient.TofuInit(directory);

                    executionOutputs[directory].Add(init);

                    if (init.ExecutionState != CommitStatusState.Success)
                    {
                        executionState = CommitStatusState.Failure;
                        continue;
                    }

                    var plan = await TofuClient.TofuPlan(directory);

                    executionOutputs[directory].Add(plan);

                    if (plan.ExecutionState != CommitStatusState.Success)
                    {
                        executionState = CommitStatusState.Failure;
                    }
                }

                var comment = $"""
                OpenTofu plan output for commit {taskRequestBody.Sha}:";
                
                """;

                foreach (var directory in executionOutputs)
                {
                    comment += $"""
                    `{directory.Key}`:
                    
                    """;

                    foreach (var executionOutput in directory.Value)
                    {
                        comment += $"""
                        <details><summary>`{executionOutput.ExecutionType}`:</summary>
                        ```
                        {executionOutput.ExecutionStdout}
                        ```
                        </details>
                        
                        """;
                    }

                    comment = comment.TrimEnd('\n');
                }

                await GitHubClient.CreateIssueComment(
                    comment,
                    repositoryFullName,
                    taskRequestBody.PullRequest,
                    taskRequestBody.InstallationId
                );

                await GitHubClient.CreateCommitStatus(
                    repositoryFullName,
                    taskRequestBody.Sha,
                    executionState.ToString().ToLower(), // TODO: https://github.com/dotnet/runtime/issues/92828
                    "Amaurot",
                    taskRequestBody.InstallationId
                );

                return Results.Ok();
            }
        );

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
