using System.IO.Compression;
using Elpis.Clients;

namespace Ktisis;

class Program
{
    static async Task Main(string[] args)
    {
        var githubPrivateKeyPath =
            Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY_PATH")
            ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY_PATH is null");

        var githubClientId =
            Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
            ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

        var repo =
            Environment.GetEnvironmentVariable("GITHUB_REPOSITORY")
            ?? throw new InvalidOperationException("GITHUB_REPOSITORY is null");

        var githubSha =
            Environment.GetEnvironmentVariable("GITHUB_REF")
            ?? throw new InvalidOperationException("GITHUB_REF is null");

        var githubInstallationId = long.Parse(
            Environment.GetEnvironmentVariable("GITHUB_INSTALLATION_ID")!
        );

        var githubClient = new GitHubClient(githubPrivateKeyPath, githubClientId);

        var zipball = await githubClient.DownloadRepositoryArchiveZip(
            repo,
            githubSha,
            githubInstallationId
        );

        var tempDirectory = Directory.CreateTempSubdirectory();

        ZipFile.ExtractToDirectory(zipball, tempDirectory.FullName);

        foreach (var directory in args)
        {
            var files =
                from file in new DirectoryInfo(
                    $"{tempDirectory.FullName}/{directory}"
                ).EnumerateFiles()
                select file.Name;

            foreach (var file in files)
            {
                await Console.Out.WriteLineAsync(file);
            }
        }
    }
}
