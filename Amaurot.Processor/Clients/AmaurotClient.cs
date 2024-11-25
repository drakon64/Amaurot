using System.IO.Compression;
using System.Text;
using Amaurot.Common.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;

namespace Amaurot.Processor.Clients;

internal static class AmaurotClient
{
    public static async Task CreateCommitStatus(
        TaskRequestBody taskRequestBody,
        string pullRequestFull,
        CommitStatusState state,
        string context
    )
    {
        var stateString = state.ToString().ToLower(); // TODO: https://github.com/dotnet/runtime/issues/92828

        await Console.Out.WriteLineAsync(
            $"Creating commit status ({stateString}) for pull request {pullRequestFull} commit {taskRequestBody.Sha}"
        );

        await Program.GitHubClient.CreateCommitStatus(
            new CreateCommitStatusRequest { State = stateString, Context = context },
            taskRequestBody
        );
    }

    public static async Task<DirectoryInfo> ExtractPullRequestZipball(
        TaskRequestBody taskRequestBody,
        string commit
    )
    {
        await Console.Out.WriteLineAsync(
            $"Downloading repository {taskRequestBody.RepositoryOwner}/{taskRequestBody.RepositoryName} at commit {commit}"
        );

        await using var zipball = await Program.GitHubClient.DownloadRepositoryArchiveZip(
            taskRequestBody,
            commit
        );

        var tempDirectory = Directory.CreateTempSubdirectory();

        ZipFile.ExtractToDirectory(zipball, tempDirectory.FullName);

        return tempDirectory;
    }

    public static async Task CreateComment(
        TaskRequestBody taskRequestBody,
        Workspace[] workspaces,
        string executionType
    )
    {
        await Console.Out.WriteLineAsync(
            $"Creating plan output comment for pull request {taskRequestBody.PullRequest} commit {taskRequestBody.Sha}"
        );

        var comment = new StringBuilder(
            $"Amaurot {executionType.ToLower()} output for commit {taskRequestBody.Sha}:\n\n---\n"
        );

        var directories = workspaces.Select(workspace => workspace.Directory).Distinct().ToArray();
        var workspacesDictionary = new Dictionary<string, Dictionary<string, Workspace>>();

        foreach (var directory in directories)
        {
            workspacesDictionary[directory] = new Dictionary<string, Workspace>();

            foreach (var workspace in workspaces)
            {
                if (workspace.Directory == directory)
                {
                    workspacesDictionary[directory][workspace.Name] = workspace;
                }
            }
        }

        foreach (var directory in workspacesDictionary)
        {
            comment.Append($"* `{directory.Key}`\n");

            foreach (var workspace in directory.Value)
            {
                comment.Append($"  * {workspace.Key}\n");

                comment.Append(
                    $"    <details><summary>Init</summary>\n\n"
                        + "    ```\n"
                        + $"    {workspace.Value.InitStdout!.Replace("\n", "\n    ")}\n"
                        + "    ```\n"
                        + "    </details>\n"
                );

                if (!string.IsNullOrWhiteSpace(workspace.Value.ExecutionStdout))
                {
                    comment.Append(
                        $"    <details><summary>{executionType}</summary>\n\n"
                            + "    ```\n"
                            + $"    {workspace.Value.ExecutionStdout.Replace("\n", "\n    ")}\n"
                            + "    ```\n"
                            + "    </details>\n"
                    );
                }
            }
        }

        if (executionType != "Plan")
            comment.Append("\n---\n\nMerging this pull request will apply these changes.");

        await Program.GitHubClient.CreateIssueComment(
            comment.ToString().TrimEnd('\n'),
            taskRequestBody
        );
    }
}
