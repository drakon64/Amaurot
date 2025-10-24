using System.Text.Json.Serialization;
using Amaurot.Processor.Client.Amaurot;

namespace Amaurot.Processor.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
[JsonSerializable(typeof(AmaurotClient.AmaurotJson))]
internal sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
