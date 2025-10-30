using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient
{
    internal sealed class AmaurotJson
    {
        public required IReadOnlyDictionary<string, Deployment> Deployments { get; init; }

        internal sealed class Deployment
        {
            public required string Path { get; init; }
            public string[]? VarFiles { get; init; }
        }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
    [JsonSerializable(typeof(AmaurotJson))]
    private sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
}
