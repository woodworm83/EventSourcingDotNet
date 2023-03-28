using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore;

internal record EventMetadata(
    [property: JsonProperty(PropertyName = "$correlationId")]
    Guid? CorrelationId,
    [property: JsonProperty(PropertyName = "$causationId")]
    Guid? CausationId);

internal sealed record EventMetadata<TAggregateId>(
        Guid? CorrelationId,
        Guid? CausationId, 
        TAggregateId? AggregateId = default)
    : EventMetadata(CorrelationId, CausationId);