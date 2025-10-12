namespace Amaurot.Processor.Client.OpenTofu;

internal partial class OpenTofuClient
{
    private readonly string _opentofu =
        Environment.GetEnvironmentVariable("OPENTOFU")
        ?? throw new InvalidOperationException("OPENTOFU is null");

    private readonly string _workingDirectory;
    private readonly string[]? _varFiles;

    private OpenTofuClient(DirectoryInfo workingDirectory, FileInfo[]? varFiles)
    {
        _workingDirectory = workingDirectory.FullName;

        if (varFiles == null)
            return;

        _varFiles = varFiles.Select(varFile => varFile.FullName).ToArray();
    }
}
