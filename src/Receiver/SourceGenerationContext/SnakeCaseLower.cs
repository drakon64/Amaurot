using System.Text.Json.Serialization;
using Amaurot.Receiver.Client;

namespace Amaurot.Receiver.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[JsonSerializable(typeof(GoogleCloudClient.AccessTokenResponse))]
internal sealed partial class SnakeCaseLowerSourceGenerationContext : JsonSerializerContext;
