using System.Text.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB;

public record EventMetadata<TAggregateId>(
    TAggregateId AggregateId,
    [property: JsonPropertyName("$correlationId")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Guid? CorrelationId,
    [property: JsonPropertyName("$causationId")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    Guid? CausationId)
    where TAggregateId : IAggregateId;