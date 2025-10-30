using Amaurot.Receiver.Client.GitHub;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient
{
    internal static async Task<string[]> GetPullRequestPaths(
        string repo,
        long number,
        long installationId
    )
    {
        return (
            from file in await GitHubClient.ListPullRequestFiles(repo, number, installationId)
            where
                file.Filename.EndsWith(".tf")
                || file.Filename.EndsWith(".tfvars")
                || file.Filename.EndsWith(".tftpl")
                || file.Filename == ".terraform.lock.hcl"
            select file.Filename[..(file.Filename.LastIndexOf('/') - 1)]
        )
            .Distinct()
            .ToArray();
    }
}
