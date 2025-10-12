using System.CommandLine;
using System.Formats.Tar;
using System.IO.Compression;
using Amaurot.Processor.Client.GitHub;

namespace Amaurot.Processor;

public static class Program
{
    internal static readonly HttpClient HttpClient = new();

    public static async Task<int> Main(string[] args)
    {
        var repository = new Argument<string>("repository")
        {
            Description = "The repository to pull from",
        };

        var pullRequest = new Argument<ulong>("pull request")
        {
            Description = "The pull request number to comment on",
        };

        var commit = new Argument<string>("commit")
        {
            Description = "The commit hash to perform a run against",
        };

        var installationId = new Argument<long>("installation id")
        {
            Description = "The installation ID of the GitHub App",
        };

        var path = new Option<string>("--path", "-p")
        {
            Description = "The subdirectory in the repository to perform the run in",
        };

        var varsFile = new Option<string[]>("--vars-file")
        {
            Description = "Path to a vars file to be used during the run",
        };

        var plan = new Command("plan", "Perform an OpenTofu plan run");

        plan.SetAction(async parseResult =>
        {
            var githubClient = new GitHubClient(
                parseResult.GetRequiredValue(repository),
                parseResult.GetRequiredValue(pullRequest),
                parseResult.GetRequiredValue(commit),
                parseResult.GetRequiredValue(installationId)
            );

            var workingDirectory = Directory.CreateTempSubdirectory();

            await ExtractRepository(githubClient, workingDirectory);

            workingDirectory.Delete(true);

            return 0;
        });

        var apply = new Command("apply", "Perform an OpenTofu apply run");

        var rootCommand = new RootCommand("Sample app for System.CommandLine");
        rootCommand.Arguments.Add(repository);
        rootCommand.Arguments.Add(pullRequest);
        rootCommand.Arguments.Add(commit);
        rootCommand.Arguments.Add(installationId);
        rootCommand.Subcommands.Add(plan);
        rootCommand.Subcommands.Add(apply);
        rootCommand.Options.Add(path);
        rootCommand.Options.Add(varsFile);

        return await rootCommand.Parse(args).InvokeAsync();

        static async Task ExtractRepository(
            GitHubClient githubClient,
            DirectoryInfo workingDirectory
        ) =>
            await TarFile.ExtractToDirectoryAsync(
                new GZipStream(
                    await githubClient.DownloadRepositoryArchive(),
                    CompressionMode.Decompress
                ),
                workingDirectory.FullName,
                false
            );
    }
}
