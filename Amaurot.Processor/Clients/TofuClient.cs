namespace Amaurot.Processor.Clients;

internal static class TofuClient
{
    internal static async Task CreateTofuStatusComment(
        string init,
        string? plan,
        string? apply,
        string? state, // TODO: https://github.com/dotnet/runtime/issues/92828
        string repositoryFullName,
        long pullRequest,
        string sha,
        long installationId
    )
    {
        var comment = $"""
            `tofu init`:
            ```
            {init}
            ```
            """;

        if (plan != null)
        {
            comment += $"""
                `tofu plan`:
                ```
                {plan}
                ```
                """;

            if (apply != null)
            {
                comment += $"""
                    `tofu apply`:
                    ```
                    {apply}
                    ```
                    """;
            }
        }

        await Program.GitHubClient.CreateIssueComment(
            comment,
            repositoryFullName,
            pullRequest,
            installationId
        );

        if (state != null)
        {
            await Program.GitHubClient.CreateCommitStatus(
                repositoryFullName,
                sha,
                state,
                "Amaurot",
                installationId
            );
        }
    }
}
