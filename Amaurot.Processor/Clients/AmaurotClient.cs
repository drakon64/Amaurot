using Amaurot.Processor.Models.Amaurot;

namespace Amaurot.Processor.Clients;

internal static class AmaurotClient
{
    public static async Task<string> Comment(AmaurotComment amaurotComment)
    {
        await Console.Out.WriteLineAsync(
            $"Creating plan output comment for pull request {amaurotComment.TaskRequestBody.PullRequest} commit {amaurotComment.TaskRequestBody.Sha}"
        );

        // TODO: Use StringBuilder
        var comment =
            $"Amaurot plan output for commit {amaurotComment.TaskRequestBody.Sha}:\n\n" + "---\n";

        foreach (var directory in amaurotComment.DirectoryOutputs)
        {
            comment += $"* `{directory.Key}`\n";

            foreach (var workspace in directory.Value)
            {
                comment += $"  * {workspace.Key}\n";

                comment +=
                    $"    <details><summary>{workspace.Value.Init.ExecutionType.ToString()}</summary>\n\n"
                    + "    ```\n"
                    + $"    {workspace.Value.Init.ExecutionStdout.Replace("\n", "\n    ")}\n"
                    + "    ```\n"
                    + "    </details>\n";

                if (workspace.Value.Execution is not null)
                {
                    comment +=
                        $"    <details><summary>{workspace.Value.Execution.ExecutionType.ToString()}</summary>\n\n"
                        + "    ```\n"
                        + $"    {workspace.Value.Execution.ExecutionStdout.Replace("\n", "\n    ")}\n"
                        + "    ```\n"
                        + "    </details>\n";
                }
            }
        }

        return comment.TrimEnd('\n');
    }
}
