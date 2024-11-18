using System.Diagnostics;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor.Clients;

internal static class TofuClient
{
    private static readonly string TofuPath = Environment.GetEnvironmentVariable("TOFU_PATH")!;

    public static async Task<PlanOutput> TofuExecution(Execution execution)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = TofuPath,
            WorkingDirectory = execution.Directory,
            RedirectStandardOutput = true,
        };

        processStartInfo.ArgumentList.Add(execution.ExecutionType.ToString().ToLower());
        processStartInfo.ArgumentList.Add("-input=false");
        processStartInfo.ArgumentList.Add("-no-color");

        var varFiles = execution.Workspace.VarFiles;

        if (varFiles is not null)
        {
            foreach (var varFile in varFiles)
            {
                processStartInfo.ArgumentList.Add($"-var-file={varFile}");
            }
        }

        var tofu = Process.Start(processStartInfo);

        await tofu!.WaitForExitAsync();

        var stdout = await tofu.StandardOutput.ReadToEndAsync();

        return new PlanOutput
        {
            ExecutionType = execution.ExecutionType,
            ExecutionState = tofu.ExitCode is 0 or 2
                ? CommitStatusState.Success
                : CommitStatusState.Failure,
            ExecutionStdout = stdout.TrimStart('\n').TrimEnd('\n'),
        };
    }
}
