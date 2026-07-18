using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Amaurot.Models;

namespace Amaurot;

class Program
{
    static async Task Main(string[] args)
    {
        var amaurotJsonFile = File.OpenRead("amaurot.json");

        var amaurotJson = await JsonSerializer.DeserializeAsync<AmaurotJson>(
            amaurotJsonFile,
            SourceGenerationContext.Default.AmaurotJson
        );

        await amaurotJsonFile.DisposeAsync();

        foreach (var deployment in amaurotJson!.Deployments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "tofu",
                ArgumentList = { "plan" },
                WorkingDirectory = deployment.Value.Path,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            if (deployment.Value.VarFiles != null)
                foreach (var varFile in deployment.Value.VarFiles)
                {
                    processStartInfo.ArgumentList.Add($"-var-file={varFile})");
                }

            var process = Process.Start(processStartInfo);

            await process!.WaitForExitAsync();

            var plan = new StringBuilder();

            var stderr = await process.StandardError.ReadToEndAsync();
            if (stderr.Length != 0)
                plan.AppendLine(await process.StandardError.ReadToEndAsync());

            plan.AppendLine(await process.StandardOutput.ReadToEndAsync());

            deployment.Value.Plan = plan.ToString();
        }
    }
}
