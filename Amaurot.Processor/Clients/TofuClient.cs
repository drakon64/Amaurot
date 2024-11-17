using System.Diagnostics;
using Amaurot.Processor.Models.GitHub.Commit;
using Amaurot.Processor.Models.OpenTofu;

namespace Amaurot.Processor.Clients;

internal static class TofuClient
{
    private static readonly string TofuPath =
        Environment.GetEnvironmentVariable("TOFU_PATH")
        ?? throw new InvalidOperationException("TOFU_PATH is null");

    private const string TofuArguments = "-input=false -no-color";

    public static async Task<PlanOutput> TofuInit(string directory)
    {
        var init = Process.Start(
            new ProcessStartInfo
            {
                FileName = TofuPath,
                Arguments = $"init {TofuArguments}",
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

    public static async Task<PlanOutput> TofuPlan(string directory)
    {
        var plan = Process.Start(
            new ProcessStartInfo
            {
                FileName = TofuPath,
                Arguments = $"plan {TofuArguments}",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
            }
        );

        await plan!.WaitForExitAsync();

        return new PlanOutput
        {
            ExecutionType = ExecutionType.Plan,
            ExecutionState =
                plan.ExitCode == 0 ? CommitStatusState.Success : CommitStatusState.Failure,
            ExecutionStdout = await plan.StandardOutput.ReadToEndAsync(),
        };
    }
}
