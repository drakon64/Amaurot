using System.Text;
using Amaurot.Processor.Models.Amaurot;
using Google.Cloud.Firestore;

namespace Amaurot.Processor.Clients;

internal static class AmaurotClient
{
    private static readonly FirestoreDb FirestoreDb = FirestoreDb.Create();

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
