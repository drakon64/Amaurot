using System.Diagnostics;

namespace Amaurot.Processor.Client.OpenTofu;

internal partial class OpenTofuClient
{
    internal async Task<RunOutput> Init()
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
            throw new Exception();

        await tofu.WaitForExitAsync();

        if (tofu.ExitCode != 0)
            throw new Exception();

        return new RunOutput
        {
            StandardOutput = await tofu.StandardOutput.ReadToEndAsync(),
            StandardError = await tofu.StandardError.ReadToEndAsync(),
        };
    }
}
