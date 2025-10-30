using System.Text.Json;
using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Client.Amaurot;

internal sealed partial class AmaurotClient
{
    private readonly string[] _amaurotPaths;

    internal AmaurotClient(FileInfo amaurotJson)
    {
        using var json = amaurotJson.OpenText();

        _amaurotPaths = (
            from deployment in JsonSerializer.Deserialize<IReadOnlyDictionary<string, Deployment>>(
                json.ReadToEnd(),
                KebabCaseLowerSourceGenerationContext.Default.IReadOnlyDictionaryStringDeployment
            )!
            select deployment.Value.Path.TrimEnd('/')
        )
            .Distinct()
            .ToArray();
    }

    internal sealed class Deployment
    {
        public required string Path { get; init; }
        public string[]? VarFiles { get; init; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
    [JsonSerializable(typeof(IReadOnlyDictionary<string, Deployment>))]
    private sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
}
