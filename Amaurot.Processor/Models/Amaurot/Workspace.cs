namespace Amaurot.Processor.Models.Amaurot;

internal class Workspace
{
    public required string Name { get; init; }
    public required string Directory { get; init; }
    public string[]? VarFiles { get; init; }
}
