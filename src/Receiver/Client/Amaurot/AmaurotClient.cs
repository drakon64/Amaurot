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
                .Deserialize<IReadOnlyDictionary<string, object>>(
                    Encoding.UTF8.GetString(amaurotJson),
                    KebabCaseLowerSourceGenerationContext.Default.IReadOnlyDictionaryStringObject
                )!
                .Keys
            select deployment
        ).ToArray();

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
    [JsonSerializable(typeof(IReadOnlyDictionary<string, object>))]
    private sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
}
