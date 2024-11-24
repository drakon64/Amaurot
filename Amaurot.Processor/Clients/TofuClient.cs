using System.Diagnostics;
using Amaurot.Processor.Models.Amaurot;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor.Clients;

internal static class TofuClient
{
    private static readonly string TofuPath = Environment.GetEnvironmentVariable("TOFU_PATH")!;

    public static async Task<ExecutionOutput> TofuExecution(
        string workingDirectory,
        Workspace workspace,
        ExecutionType executionType
    )
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = TofuPath,
            WorkingDirectory = $"{workingDirectory}/{workspace.Directory}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        processStartInfo.ArgumentList.Add(executionType.ToString().ToLower());
        processStartInfo.ArgumentList.Add("-input=false");
        processStartInfo.ArgumentList.Add("-no-color");

        string? planOutPath = null;

        if (executionType == ExecutionType.Plan)
        {
            planOutPath = Path.GetTempFileName();
            processStartInfo.ArgumentList.Add("-detailed-exitcode");
            processStartInfo.ArgumentList.Add($"-out={planOutPath}");
        }

        if (workspace.VarFiles is not null)
        {
            foreach (var varFile in workspace.VarFiles)
            {
                processStartInfo.ArgumentList.Add($"-var-file={varFile}");
            }
        }

        using var tofu = Process.Start(processStartInfo);

        await tofu!.WaitForExitAsync();

        var stdout = await tofu.StandardOutput.ReadToEndAsync();

        byte[]? planOut = null;

        if (planOutPath is not null)
        {
            if (tofu.ExitCode == 2)
                planOut = await File.ReadAllBytesAsync(planOutPath);

            File.Delete(planOutPath);
        }

        return new ExecutionOutput
        {
            ExecutionType = executionType,
            ExecutionState = tofu.ExitCode is 0 or 2
                ? CommitStatusState.Success
                : CommitStatusState.Failure,
            ExecutionStdout = stdout.TrimStart('\n').TrimEnd('\n'),
            PlanOut = planOut,
        };
    }
}
