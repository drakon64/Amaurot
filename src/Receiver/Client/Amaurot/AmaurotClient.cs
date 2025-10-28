using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient
{
    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
    [JsonSerializable(typeof(AmaurotJson))]
    private sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
}
