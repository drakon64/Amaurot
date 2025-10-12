using System.Diagnostics;

namespace Amaurot.Processor.Client.OpenTofu;

internal partial class OpenTofuClient
{
    internal async Task<InitOutput> Init()
    {
        var processStartInfo = CreateProcessStartInfo();

        processStartInfo.ArgumentList.Add("init");
        processStartInfo.ArgumentList.Add("-input=false");
        processStartInfo.ArgumentList.Add("-no-color");

        if (_varFiles != null)
            foreach (var varFile in _varFiles)
                processStartInfo.ArgumentList.Add(varFile);

        using var tofu = Process.Start(processStartInfo);

        if (tofu == null)
            throw new Exception(); // TODO: Useful exception

        await tofu.WaitForExitAsync();

        return new InitOutput
        {
            ExitCode = tofu.ExitCode,
            StandardOutput = await tofu.StandardOutput.ReadToEndAsync(),
            StandardError = await tofu.StandardError.ReadToEndAsync(),
        };
    }

    internal sealed class InitOutput : RunOutput
    {
        public required int ExitCode { get; init; }
    }
}
