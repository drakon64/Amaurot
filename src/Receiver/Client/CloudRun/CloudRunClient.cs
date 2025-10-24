namespace Amaurot.Receiver.Client.CloudRun;

internal static class CloudRunClient
{
    internal sealed class ExecuteJob
    {
        public required Overrides Overrides { get; init; }
    }

    internal sealed class Overrides
    {
        public required ContainerOverride[] ContainerOverrides { get; init; }
    }

    internal sealed class ContainerOverride
    {
        public required string[] Args { get; init; }
    }
}
