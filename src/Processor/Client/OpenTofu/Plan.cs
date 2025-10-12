using System.Diagnostics;

namespace Amaurot.Processor.Client.OpenTofu;

internal partial class OpenTofuClient
{
    private static readonly int[] SucceededExitCodes = [0, 2];

    internal async Task<RunOutput> Plan()
    {
        var outFile = Path.GetTempFileName();

        var processStartInfo = CreateProcessStartInfo();

        processStartInfo.ArgumentList.Add("plan");

        if (_varFiles != null)
            foreach (var varFile in _varFiles)
                processStartInfo.ArgumentList.Add(varFile);

        processStartInfo.ArgumentList.Add("-detailed-exitcode");
        processStartInfo.ArgumentList.Add("-input=false");
        processStartInfo.ArgumentList.Add("-no-color");
        processStartInfo.ArgumentList.Add("-concise");
        processStartInfo.ArgumentList.Add($"-out={outFile}");

        using var tofu = Process.Start(processStartInfo);

        if (tofu == null)
            throw new Exception(); // TODO: Useful exception

        await tofu.WaitForExitAsync();

        if (!SucceededExitCodes.Contains(tofu.ExitCode))
            return new RunOutput
            {
                StandardOutput = await tofu.StandardOutput.ReadToEndAsync(),
                StandardError = await tofu.StandardError.ReadToEndAsync(),
            };

        var planOut = await File.ReadAllBytesAsync(outFile);
        File.Delete(outFile);

        return new PlanOutput
        {
            Out = planOut,
            StandardOutput = await tofu.StandardOutput.ReadToEndAsync(),
            StandardError = await tofu.StandardError.ReadToEndAsync(),
        };
    }

    internal sealed class PlanOutput : RunOutput
    {
        public required byte[] Out { get; init; }
    }
}
