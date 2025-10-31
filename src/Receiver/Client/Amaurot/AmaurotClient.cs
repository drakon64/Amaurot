using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient(byte[] amaurotJson)
{
    private readonly string[] _amaurotPaths = (
        from deployment in JsonSerializer.Deserialize<IReadOnlyDictionary<string, Deployment>>(
            Encoding.UTF8.GetString(amaurotJson),
            KebabCaseLowerSourceGenerationContext.Default.IReadOnlyDictionaryStringDeployment
        )!
        select deployment.Value.Path.TrimEnd('/')
    )
        .Distinct()
        .ToArray();

    internal sealed class Deployment
    {
        public required string Path { get; init; }
        public string[]? VarFiles { get; init; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
    [JsonSerializable(typeof(IReadOnlyDictionary<string, Deployment>))]
    private sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
}
