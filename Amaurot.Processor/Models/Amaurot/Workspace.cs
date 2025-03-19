namespace Amaurot.Processor.Models.Amaurot;

internal class Workspace
{
    public required string Name { get; init; }
    public required string Directory { get; init; }
    public string[]? VarFiles { get; init; }
    public byte[]? PlanOut { get; set; }
    public string? InitStdout { get; set; }
    public string? ExecutionStdout { get; set; }
}
