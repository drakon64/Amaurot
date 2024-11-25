using System.IO.Compression;
using System.Text;
using Amaurot.Common.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;
using Google.Cloud.Firestore;

namespace Amaurot.Processor.Clients;

internal static class AmaurotClient
{
    private static readonly FirestoreDb FirestoreDatabase = FirestoreDb.Create();

    public static async Task<ChangedWorkspaces> GetWorkspaces(
        TaskRequestBody taskRequestBody,
        string pullRequestFull
    )
    {
        await Console.Out.WriteLineAsync($"Getting mergeability of pull request {pullRequestFull}");

        string? mergeCommitSha;

        while (true)
        {
            var pullRequest = (await Program.GitHubClient.GetPullRequest(taskRequestBody))!;

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

        var pullRequestFiles = await Program.GitHubClient.ListPullRequestFiles(taskRequestBody);

        var changedDirectories = (
            from file in pullRequestFiles
            let lastIndex = file.FileName.LastIndexOf('/')
            select lastIndex != -1 ? file.FileName.Remove(lastIndex) : ""
        )
            .Distinct()
            .ToArray();

        var changedTfVars = (
            from file in pullRequestFiles
            where file.FileName.EndsWith(".tfvars") || file.FileName.EndsWith(".tfvars.json")
            select file.FileName
        ).ToArray();

        if (changedDirectories.Length == 0)
        {
            throw new Exception($"Pull request {pullRequestFull} is empty");
        }

        await Console.Out.WriteLineAsync(
            $"Getting changed workspaces in pull request {pullRequestFull}"
        );

        var amaurotJson = await Program.GitHubClient.GetRepositoryAmaurotJson(
            taskRequestBody,
            mergeCommitSha
        );

        var workspaces = (
            from changedDirectory in changedDirectories
            from changedTfVar in changedTfVars
            from workspace in amaurotJson.Workspaces
            where
                workspace.Directory == changedDirectory
                || (workspace.VarFiles is not null && workspace.VarFiles.Contains(changedTfVar))
            select workspace
        )
            .Distinct()
            .ToArray();

        if (workspaces.Length == 0)
        {
            throw new Exception($"Pull request {pullRequestFull} contains no modified workspaces");
        }

        return new ChangedWorkspaces { Workspaces = workspaces, MergeCommitSha = mergeCommitSha };
    }

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

        await Program.GitHubClient.CreateIssueComment(
            comment.ToString().TrimEnd('\n'),
            taskRequestBody
        );
    }

    public static async Task SavePlanOutput(string headSha, SavedWorkspaces savedWorkspaces)
    {
        await FirestoreDatabase.Collection("plans").Document(headSha).SetAsync(savedWorkspaces);
    }

    public static async Task<SavedWorkspaces> GetSavedPlanOutput(string headSha)
    {
        var documentSnapshot = await FirestoreDatabase
            .Collection("plans")
            .Document(headSha)
            .GetSnapshotAsync();

        return documentSnapshot.ConvertTo<SavedWorkspaces>();
    }

    public static async Task DeleteSavedPlans(string pullRequest)
    {
        var savedPlans = await FirestoreDatabase
            .Collection("plans")
            .WhereEqualTo("PullRequest", pullRequest)
            .GetSnapshotAsync();

        foreach (var savedPlan in savedPlans.Documents)
        {
            await savedPlan.Reference.DeleteAsync();
        }
    }
}
