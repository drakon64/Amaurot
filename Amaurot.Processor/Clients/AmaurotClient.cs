using System.Text;
using Amaurot.Common.Models;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
using Google.Cloud.Firestore;

namespace Amaurot.Processor.Clients;

internal static class AmaurotClient
{
    private static readonly FirestoreDb FirestoreDb = FirestoreDb.Create();

    public static async Task<AmaurotWorkspaceRedo[]> GetWorkspaces(
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

        var changedDirectories = (
            from file in await Program.GitHubClient.ListPullRequestFiles(taskRequestBody)
            let lastIndex = file.FileName.LastIndexOf('/')
            select lastIndex != -1 ? file.FileName.Remove(lastIndex) : file.FileName
        )
            .Distinct()
            .ToArray();

        if (changedDirectories.Length == 0)
        {
            throw new Exception($"Pull request {pullRequestFull} is empty");
        }

        await Console.Out.WriteLineAsync(
            $"Getting changed workspaces in pull request {pullRequestFull}"
        );

        var amaurotJson = await Program.GitHubClient.GetRepositoryAmaurotJson(taskRequestBody);
        var workspaces = new List<AmaurotWorkspaceRedo>();

        foreach (var changedDirectory in changedDirectories)
        {
            if (amaurotJson.Workspaces.TryGetValue(changedDirectory, out var workspace))
            {
                workspaces.Add(workspace);
            }
        }

        if (workspaces.Count == 0)
        {
            throw new Exception($"Pull request {pullRequestFull} contains no modified workspaces");
        }

        return workspaces.ToArray();
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
