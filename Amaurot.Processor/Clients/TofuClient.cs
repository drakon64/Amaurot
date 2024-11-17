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

        return new PlanOutput
        {
            ExecutionType = ExecutionType.Init,
            ExecutionState =
                init.ExitCode == 0 ? CommitStatusState.Success : CommitStatusState.Failure,
            ExecutionStdout = await init.StandardOutput.ReadToEndAsync(),
        };
    }
}
