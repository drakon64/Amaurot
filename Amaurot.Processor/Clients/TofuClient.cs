using System.Diagnostics;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor.Clients;

internal static class TofuClient
{
    private static readonly string TofuPath = Environment.GetEnvironmentVariable("TOFU_PATH")!;

    private const string TofuArguments = "-input=false -no-color";

    public static async Task<PlanOutput> TofuExecution(
        ExecutionType executionType,
        string directory
    )
    {
        var init = Process.Start(
            new ProcessStartInfo
            {
                FileName = TofuPath,
                Arguments = $"{executionType.ToString().ToLower()} {TofuArguments}",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
            }
        );

        await init!.WaitForExitAsync();

        var stdout = await init.StandardOutput.ReadToEndAsync();

        return new PlanOutput
        {
            ExecutionType = executionType,
            ExecutionState =
                init.ExitCode == 0 ? CommitStatusState.Success : CommitStatusState.Failure,
            ExecutionStdout = stdout.TrimStart('\n').TrimEnd('\n'),
        };
    }
}
