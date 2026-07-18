using System.Text.Json.Serialization;

namespace Amaurot.Models;

internal class AmaurotJson
{
    public required Dictionary<string, Deployment> Deployments { get; init; }

    internal class Deployment
    {
        public required string Path { get; init; }
        public string[]? VarFiles { get; init; }

        [JsonIgnore]
        public string? Plan { get; set; }
    }
}

[JsonSerializable(typeof(AmaurotJson))]
internal partial class SourceGenerationContext : JsonSerializerContext;
