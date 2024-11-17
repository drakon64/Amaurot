using System.IO.Compression;
using Amaurot.Lib.Clients;

var githubPrivateKey =
    Environment.GetEnvironmentVariable("GITHUB_PRIVATE_KEY")
    ?? throw new InvalidOperationException("GITHUB_PRIVATE_KEY is null");

var githubClientId =
    Environment.GetEnvironmentVariable("GITHUB_CLIENT_ID")
    ?? throw new InvalidOperationException("GITHUB_CLIENT_ID is null");

var repoOwner =
    Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_OWNER")
    ?? throw new InvalidOperationException("GITHUB_REPOSITORY_OWNER is null");

var repoName =
    Environment.GetEnvironmentVariable("GITHUB_REPOSITORY_NAME")
    ?? throw new InvalidOperationException("GITHUB_REPOSITORY_NAME is null");

var githubSha =
    Environment.GetEnvironmentVariable("GITHUB_REF")
    ?? throw new InvalidOperationException("GITHUB_REF is null");

var githubInstallationId = long.Parse(
    Environment.GetEnvironmentVariable("GITHUB_INSTALLATION_ID")!
);

var githubClient = new GitHubClient(githubPrivateKey, githubClientId);

var zipball = await githubClient.DownloadRepositoryArchiveZip(
    $"{repoOwner}/{repoName}",
    githubSha,
    githubInstallationId
);

var tempDirectory = Directory.CreateTempSubdirectory();

ZipFile.ExtractToDirectory(zipball, tempDirectory.FullName);

foreach (var directory in args)
{
    var files =
        from file in new DirectoryInfo(
            $"{tempDirectory.FullName}/{repoOwner}-{repoName}-{githubSha}/{directory}"
        ).EnumerateFiles()
        select file.Name;

    foreach (var file in files)
    {
        await Console.Out.WriteLineAsync($"{directory}/{file}");
    }
}
