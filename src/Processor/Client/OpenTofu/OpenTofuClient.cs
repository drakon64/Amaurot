using System.Diagnostics;

namespace Amaurot.Processor.Client.OpenTofu;

internal partial class OpenTofuClient
{
    private readonly string _opentofu =
        Environment.GetEnvironmentVariable("OPENTOFU")
        ?? throw new InvalidOperationException("OPENTOFU is null");

    private readonly string _workingDirectory;
    private readonly string[]? _varFiles;

    public OpenTofuClient(DirectoryInfo workingDirectory, FileInfo[]? varFiles)
    {
        _workingDirectory = workingDirectory.FullName;

        if (varFiles == null)
            return;

        _varFiles = varFiles.Select(varFile => $"-var-file={varFile.FullName}").ToArray();
    }

    private ProcessStartInfo CreateProcessStartInfo() =>
        new() { FileName = _opentofu, WorkingDirectory = _workingDirectory };

    internal class RunOutput
    {
        public required string StandardOutput { get; init; }
        public required string StandardError { get; init; }
    }
}
