using System.IO.Compression;
using System.Text;
using Amaurot.Common.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
using Google.Cloud.Firestore;

namespace Amaurot.Processor.Clients;

internal static class AmaurotClient
{
    private static readonly FirestoreDb FirestoreDb = FirestoreDb.Create();

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
                workspace.Directory == changedDirectory || workspace.VarFiles.Contains(changedTfVar)
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
        CommitStatusState state
    )
    {
        var stateString = state.ToString().ToLower(); // TODO: https://github.com/dotnet/runtime/issues/92828

        await Console.Out.WriteLineAsync(
            $"Creating commit status ({stateString}) for pull request {pullRequestFull} commit {taskRequestBody.Sha}"
        );

        await Program.GitHubClient.CreateCommitStatus(
            new CreateCommitStatusRequest { State = stateString, Context = Program.GitHubContext },
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

    public static async Task<string> Comment(AmaurotComment amaurotComment)
    {
        await Console.Out.WriteLineAsync(
            $"Creating plan output comment for pull request {amaurotComment.TaskRequestBody.PullRequest} commit {amaurotComment.TaskRequestBody.Sha}"
        );

        // TODO: Use StringBuilder
        var comment = new StringBuilder(
            $"Amaurot plan output for commit {amaurotComment.TaskRequestBody.Sha}:\n\n" + "---\n"
        );

        foreach (var directory in amaurotComment.DirectoryOutputs)
        {
            comment.Append($"* `{directory.Key}`\n");

            foreach (var workspace in directory.Value)
            {
                comment.Append($"  * {workspace.Key}\n");

                comment.Append(
                    $"    <details><summary>{workspace.Value.Init.ExecutionType.ToString()}</summary>\n\n"
                        + "    ```\n"
                        + $"    {workspace.Value.Init.ExecutionStdout.Replace("\n", "\n    ")}\n"
                        + "    ```\n"
                        + "    </details>\n"
                );

                if (workspace.Value.Execution is not null)
                {
                    comment.Append(
                        $"    <details><summary>{workspace.Value.Execution.ExecutionType.ToString()}</summary>\n\n"
                            + "    ```\n"
                            + $"    {workspace.Value.Execution.ExecutionStdout.Replace("\n", "\n    ")}\n"
                            + "    ```\n"
                            + "    </details>\n"
                    );
                }
            }
        }

        return comment.ToString().TrimEnd('\n');
    }

    public static async Task SavePlanOutput(SavedPlan savedPlan)
    {
        await FirestoreDb.Collection("plans").AddAsync(savedPlan);
    }

    public static async Task<SavedPlan[]> GetSavedPlanOutput(SavedPlanQuery savedPlanQuery)
    {
        var snapshot = await FirestoreDb
            .Collection("plans")
            .WhereEqualTo("PullRequest", $"{savedPlanQuery.PullRequest}")
            .WhereEqualTo("Sha", savedPlanQuery.Sha)
            .GetSnapshotAsync();

        return snapshot.Select(document => document.ConvertTo<SavedPlan>()).ToArray();
    }
}
