using System.IO.Compression;
using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models.GitHub.PullRequest;

var githubPrivateKey =
    Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
    ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

var githubClientId =
    Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
    ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

var builder = WebApplication.CreateSlimBuilder();
var app = builder.Build();

var githubClient = new GitHubClient(githubPrivateKey, githubClientId);

app.MapGet("/", () => Results.Ok());

app.MapPost(
    "/plan",
    async (TaskRequestBody taskRequestBody) =>
    {
        var repositoryFullName =
            $"{taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName}";

        var tfDirectories = (
            from file in await githubClient.ListPullRequestFiles(
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
            select lastIndex != -1 ? file.FileName.Remove(lastIndex) : ""
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
                await githubClient.GetPullRequest(
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

        await githubClient.CreateCommitStatus(
            repositoryFullName,
            taskRequestBody.Sha,
            // CommitStatusState.Pending,
            "pending",
            "Amaurot",
            taskRequestBody.InstallationId
        );

        var zipball = await githubClient.DownloadRepositoryArchiveZip(
            repositoryFullName,
            taskRequestBody.Sha,
            taskRequestBody.InstallationId
        );

        var tempDirectory = Directory.CreateTempSubdirectory();

        ZipFile.ExtractToDirectory(zipball, tempDirectory.FullName);

        foreach (var directory in args)
        {
            var files =
                from file in new DirectoryInfo(
                    $"{tempDirectory.FullName}/{taskRequestBody.RepositoryOwner}-{taskRequestBody.RepositoryName}-{taskRequestBody.Sha}/{directory}"
                ).EnumerateFiles()
                select file.Name;

            foreach (var file in files)
            {
                await Console.Out.WriteLineAsync($"{directory}/{file}");
            }
        }

        return Results.Ok();
    }
);

app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
