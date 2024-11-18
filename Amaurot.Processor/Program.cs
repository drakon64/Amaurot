using System.IO.Compression;
using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.GitHub.PullRequest;
using Amaurot.Processor.Models.OpenTofu;

var githubPrivateKey =
    Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
    ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

var githubClientId =
    Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
    ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

var gitHubClient = new GitHubClient(githubPrivateKey, githubClientId);

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
            from file in await gitHubClient.ListPullRequestFiles(
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
                await gitHubClient.GetPullRequest(
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

        await gitHubClient.CreateCommitStatus(
            repositoryFullName,
            taskRequestBody.Sha,
            "pending", // TODO: https://github.com/dotnet/runtime/issues/92828
            taskRequestBody.InstallationId
        );

        var zipball = await gitHubClient.DownloadRepositoryArchiveZip(
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

            var init = await TofuClient.TofuExecution(ExecutionType.Init, directory);

            executionOutputs.Add(tfDirectory, [init]);

            if (init.ExecutionState != CommitStatusState.Success)
            {
                executionState = CommitStatusState.Failure;
                continue;
            }

            var plan = await TofuClient.TofuExecution(ExecutionType.Plan, directory);

            executionOutputs[tfDirectory].Add(plan);

            if (plan.ExecutionState != CommitStatusState.Success)
            {
                executionState = CommitStatusState.Failure;
            }
        }

        var comment = $"OpenTofu plan output for commit {taskRequestBody.Sha}:\n\n";

        foreach (var directory in executionOutputs)
        {
            comment += $"`{directory.Key}`:\n";

            foreach (var executionOutput in directory.Value)
            {
                comment += $"""
                <details><summary>{executionOutput.ExecutionType.ToString()}:</summary>

                ```
                {executionOutput.ExecutionStdout}
                ```
                </details>
                
                """;
            }
        }
        
        comment = comment.TrimEnd('\n');

        await gitHubClient.CreateIssueComment(
            comment,
            repositoryFullName,
            taskRequestBody.PullRequest,
            taskRequestBody.InstallationId
        );

        await gitHubClient.CreateCommitStatus(
            repositoryFullName,
            taskRequestBody.Sha,
            executionState.ToString().ToLower(), // TODO: https://github.com/dotnet/runtime/issues/92828
            taskRequestBody.InstallationId
        );

        return Results.Ok();
    }
);

app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
