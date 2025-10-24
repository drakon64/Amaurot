using System.Text.Json.Serialization;
using Amaurot.Receiver.Client.CloudRun;

namespace Amaurot.Receiver.SourceGenerationContext;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CloudRunClient.ExecuteJob))]
internal sealed partial class CamelCaseSourceGenerationContext : JsonSerializerContext;
