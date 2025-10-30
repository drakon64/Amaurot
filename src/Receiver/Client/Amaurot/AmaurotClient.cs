using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient(FileInfo amaurotJson)
{
    private readonly string[] _amaurotPaths = (
        from deployment in JsonSerializer
            .Deserialize<AmaurotJson>(
                amaurotJson.OpenText().ReadToEnd(),
                KebabCaseLowerSourceGenerationContext.Default.AmaurotJson
            )!
            .Deployments
        select deployment.Value.Path.TrimEnd('/')
    )
        .Distinct()
        .ToArray();

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
