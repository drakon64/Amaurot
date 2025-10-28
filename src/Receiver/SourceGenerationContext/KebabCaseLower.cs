using System.Text.Json.Serialization;

using Amaurot.Receiver.Client.Amaurot;

namespace Amaurot.Receiver.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.KebabCaseLower)]
[JsonSerializable(typeof(AmaurotClient.AmaurotJson))]
internal sealed partial class KebabCaseLowerSourceGenerationContext : JsonSerializerContext;
