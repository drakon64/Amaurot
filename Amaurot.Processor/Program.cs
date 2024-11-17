using System.Diagnostics;
using System.IO.Compression;
using Amaurot.Common.Models;
using Amaurot.Processor.Clients;
using Amaurot.Processor.Models.GitHub.PullRequest;

namespace Amaurot.Processor;

public class Program
{
    private static readonly string GithubPrivateKey =
        Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
        ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

    private static readonly string GithubClientId =
        Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
        ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

    internal static readonly GitHubClient GitHubClient = new(GithubPrivateKey, GithubClientId);

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

                foreach (var tfDirectory in tfDirectories)
                {
                    var directory =
                        $"{tempDirectory.FullName}/{taskRequestBody.RepositoryOwner}-{taskRequestBody.RepositoryName}-{taskRequestBody.Sha}/{tfDirectory}";

                    var tofu = Environment.GetEnvironmentVariable("TOFU_PATH");
                    const string tofuArguments = "-input=false -no-color";
                    string? state = null; // TODO: https://github.com/dotnet/runtime/issues/92828

                    var init = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = tofu,
                            Arguments = $"init {tofuArguments}",
                            WorkingDirectory = directory,
                            RedirectStandardOutput = true,
                        }
                    );

                    await init!.WaitForExitAsync();

                    var initStdout = await init.StandardOutput.ReadToEndAsync();

                    if (init.ExitCode != 0)
                    {
                        state = "failure"; // TODO: https://github.com/dotnet/runtime/issues/92828
                    }

                    var plan = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = tofu,
                            Arguments = $"plan {tofuArguments}",
                            WorkingDirectory = directory,
                            RedirectStandardOutput = true,
                        }
                    );

                    await plan!.WaitForExitAsync();

                    var planStdout = await plan.StandardOutput.ReadToEndAsync();

                    state = plan.ExitCode == 0 ? "success" : "failure"; // TODO: https://github.com/dotnet/runtime/issues/92828

                    await TofuClient.CreateTofuStatusComment(
                        initStdout,
                        planStdout,
                        null,
                        state,
                        repositoryFullName,
                        taskRequestBody.PullRequest,
                        taskRequestBody.Sha,
                        taskRequestBody.InstallationId
                    );
                }

                return Results.Ok();
            }
        );

        app.Run($"http://*:{Environment.GetEnvironmentVariable("PORT")}");
    }
}
