using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Amaurot.Models;

var amaurotJsonFile = File.OpenRead("amaurot.json");

var amaurotJson = await JsonSerializer.DeserializeAsync<AmaurotJson>(
    amaurotJsonFile,
    SourceGenerationContext.Default.AmaurotJson
);

await amaurotJsonFile.DisposeAsync();

var plan = new StringBuilder();
var failed = false;

foreach (var deployment in amaurotJson!.Deployments)
{
    var tofuProcessStartInfo = new ProcessStartInfo
    {
        FileName = "tofu",
        ArgumentList = { "plan", "-detailed-exitcode", "-input=false", "-no-color" },
        WorkingDirectory = deployment.Value.Path,
        RedirectStandardError = true,
        RedirectStandardOutput = true,
    };

    if (deployment.Value.VarFiles != null)
        foreach (var varFile in deployment.Value.VarFiles)
        {
            tofuProcessStartInfo.ArgumentList.Add($"-var-file={varFile})");
        }

    var process = Process.Start(tofuProcessStartInfo);
    await process!.WaitForExitAsync();

    if (process.ExitCode == 1)
        failed = true;

    plan.AppendLine("${deployment.Key}:");
    plan.AppendLine("```");
    var stderr = await process.StandardError.ReadToEndAsync();
    if (stderr.Length != 0)
        plan.AppendLine(stderr);
    plan.AppendLine(await process.StandardOutput.ReadToEndAsync());
    plan.Append("```");
}

var ghProcessStartInfo = new ProcessStartInfo
{
    FileName = "gh",
    ArgumentList =
    {
        "pr",
        "comment",
        Environment.GetEnvironmentVariable("GITHUB_HEAD_REF")!,
        "--body",
        plan.ToString(),
    },
};

var gh = Process.Start(ghProcessStartInfo);
await gh!.WaitForExitAsync();

return failed ? 1 : 0;
