using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient
{
    internal readonly string[] Deployments;

    internal AmaurotClient(byte[] amaurotJson) =>
        Deployments = (
            from deployment in JsonSerializer
                .Deserialize<IReadOnlyDictionary<string, Deployment>>(
                    Encoding.UTF8.GetString(amaurotJson),
                    KebabCaseLowerSourceGenerationContext
                        .Default
                        .IReadOnlyDictionaryStringDeployment
                )!
                .Keys
            select deployment
        ).ToArray();

    internal sealed class Deployment
    {
        public required string Path { get; init; }
        public string[]? VarFiles { get; init; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
    [JsonSerializable(typeof(IReadOnlyDictionary<string, Deployment>))]
    private sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
}
