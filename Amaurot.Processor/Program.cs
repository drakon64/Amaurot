using System.IO.Compression;
using System.Text.Json;
using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
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
        var tfDirectories = (
            from file in await gitHubClient.ListPullRequestFiles(taskRequestBody)
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

        var pullRequestNumber =
            $"{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}#{taskRequestBody.PullRequest}";

        if (tfDirectories.Length == 0)
        {
            var result =
                $"Pull request {pullRequestNumber} contains no OpenTofu configuration files";

            await Console.Out.WriteLineAsync(result);
            return Results.Ok(result);
        }

        await Console.Out.WriteLineAsync(
            $"Getting mergeability of pull request {pullRequestNumber}"
        );

        string? mergeCommitSha;

        while (true)
        {
            var pullRequest = (await gitHubClient.GetPullRequest(taskRequestBody))!;

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
            var result = $"Pull request {pullRequestNumber} is not mergeable";

            await Console.Out.WriteLineAsync(result);

            return Results.Ok(result);
        }

        await Console.Out.WriteLineAsync(
            $"Creating commit status (pending) for pull request {pullRequestNumber} commit {taskRequestBody.Sha}"
        );

        await gitHubClient.CreateCommitStatus(
            new CreateCommitStatusRequest
            {
                State = CommitStatusState.Pending.ToString().ToLower(), // TODO: https://github.com/dotnet/runtime/issues/92828
                Context = "Amaurot",
            },
            taskRequestBody
        );

        var zipball = await gitHubClient.DownloadRepositoryArchiveZip(taskRequestBody);

        var tempDirectory = Directory.CreateTempSubdirectory();

        ZipFile.ExtractToDirectory(zipball, tempDirectory.FullName);

        var directoryOutputs = new Dictionary<string, Dictionary<string, ExecutionOutputs>>();
        var executionState = CommitStatusState.Success;

        foreach (var tfDirectory in tfDirectories)
        {
            var directory =
                $"{tempDirectory.FullName}/{taskRequestBody.RepositoryOwner}-{taskRequestBody.RepositoryName}-{taskRequestBody.Sha}/{tfDirectory}";

            AmaurotJson amaurotJson;

            try
            {
                await using var workspacesFile = File.OpenRead($"{directory}/amaurot.json");
                amaurotJson = (
                    await JsonSerializer.DeserializeAsync<AmaurotJson>(
                        workspacesFile,
                        AmaurotSerializerContext.Default.AmaurotJson
                    )
                )!;
            }
            catch (FileNotFoundException)
            {
                await Console.Out.WriteLineAsync(
                    $"Directory {directory} doesn't contain an `amaurot.json` file"
                );

                continue;
            }

            directoryOutputs.Add(tfDirectory, new Dictionary<string, ExecutionOutputs>());

            foreach (var workspace in amaurotJson.Workspaces)
            {
                await Console.Out.WriteLineAsync($"Running OpenTofu for workspace {workspace.Key}");

                var init = await TofuClient.TofuExecution(
                    new Execution
                    {
                        ExecutionType = ExecutionType.Init,
                        Directory = directory,
                        Workspace = workspace.Value,
                    }
                );

                ExecutionOutput? plan = null;

                if (init.ExecutionState == CommitStatusState.Success)
                {
                    plan = await TofuClient.TofuExecution(
                        new Execution
                        {
                            ExecutionType = ExecutionType.Plan,
                            Directory = directory,
                            Workspace = workspace.Value,
                        }
                    );

                    if (plan.ExecutionState != CommitStatusState.Success)
                    {
                        executionState = CommitStatusState.Failure;
                    }

                    if (plan.PlanOut is not null)
                    {
                        await AmaurotClient.SavePlanOutput(
                            new SavedPlan
                            {
                                PullRequest = pullRequestNumber,
                                Sha = taskRequestBody.Sha,
                                Directory = tfDirectory,
                                Workspace = workspace.Key,
                                PlanOut = plan.PlanOut,
                            }
                        );
                    }
                }
                else
                {
                    executionState = CommitStatusState.Failure;
                }

                directoryOutputs[tfDirectory]
                    .Add(workspace.Key, new ExecutionOutputs { Init = init, Execution = plan });
            }
        }

        tempDirectory.Delete(true);

        await gitHubClient.CreateIssueComment(
            await AmaurotClient.Comment(
                new AmaurotComment
                {
                    TaskRequestBody = taskRequestBody,
                    DirectoryOutputs = directoryOutputs,
                }
            ),
            taskRequestBody
        );

        await Console.Out.WriteLineAsync(
            $"Creating commit status ({executionState.ToString().ToLower()}) for pull request {pullRequestNumber} commit {taskRequestBody.Sha}"
        );

        await gitHubClient.CreateCommitStatus(
            new CreateCommitStatusRequest
            {
                State = executionState.ToString().ToLower(), // TODO: https://github.com/dotnet/runtime/issues/92828
                Description =
                    executionState == CommitStatusState.Success
                        ? "All OpenTofu plans passed"
                        : "Some OpenTofu plans failed",
                Context = "Amaurot",
            },
            taskRequestBody
        );

        return Results.Ok();
    }
);

app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
