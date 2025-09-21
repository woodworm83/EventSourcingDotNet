using System.Text.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(EventMetadata))]
internal sealed partial class EventMetadataSerializerContext : JsonSerializerContext;