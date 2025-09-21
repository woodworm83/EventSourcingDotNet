using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB;

internal record EventMetadata(
    JsonElement AggregateId,
    [property: JsonPropertyName("$correlationId")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Guid? CorrelationId,
    [property: JsonPropertyName("$causationId")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Guid? CausationId);