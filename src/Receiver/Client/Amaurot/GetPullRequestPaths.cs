using Amaurot.Receiver.Client.GitHub;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient
{
    internal string[] GetPullRequestPaths(GitHubClient.PullRequestFile[] files)
    {
        return (
            from file in files
            where
                _amaurotPaths.Contains(file.Filename[..(file.Filename.LastIndexOf('/') - 1)])
                && (
                    file.Filename.EndsWith(".tf")
                    || file.Filename.EndsWith(".tfvars")
                    || file.Filename.EndsWith(".tftpl")
                    || file.Filename == ".terraform.lock.hcl"
                )
            select file.Filename[..(file.Filename.LastIndexOf('/') - 1)]
        )
            .Distinct()
            .ToArray();
    }
}
