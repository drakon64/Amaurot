using System.Diagnostics;

namespace Amaurot.Processor.Client.OpenTofu;

internal partial class OpenTofuClient
{
    private readonly string _opentofu =
        Environment.GetEnvironmentVariable("OPENTOFU")
        ?? throw new InvalidOperationException("OPENTOFU is null");

    private readonly string _workingDirectory;
    private readonly string? _path;
    private readonly string[]? _varFiles;

    internal OpenTofuClient(DirectoryInfo workingDirectory, string? path, string[]? varFiles)
    {
        _workingDirectory = workingDirectory.FullName;
        _path = path;

        _varFiles = varFiles
            ?.Select(varFile =>
                $"-var-file={new FileInfo(Path.Join(_workingDirectory, varFile)).FullName}"
            )
            .ToArray();
    }

    private ProcessStartInfo CreateProcessStartInfo() =>
        new() { FileName = _opentofu, WorkingDirectory = Path.Join(_workingDirectory, _path) };

    internal class RunOutput
    {
        public required string StandardOutput { get; init; }
        public required string StandardError { get; init; }
    }
}
