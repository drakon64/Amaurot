using System.Text.Json.Serialization;

namespace Amaurot.Receiver.Models;

[JsonSerializable(typeof(TaskRequestBody))]
internal partial class AmaurotJsonSerializerContext : JsonSerializerContext;
