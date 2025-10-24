namespace Amaurot.Processor.Client.Amaurot;

internal static class AmaurotClient
{
    internal sealed class AmaurotJson
    {
        public required Dictionary<string, Deployment> Deployments { get; init; }
    }

    internal sealed class Deployment
    {
        public required string Path { get; init; }
        public string[]? VarFiles { get; init; }
    }
}
