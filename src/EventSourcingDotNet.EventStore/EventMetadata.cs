using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore;

internal sealed record EventMetadata<TAggregateId>(
    TAggregateId AggregateId,
    [property: JsonProperty(PropertyName = "$correlationId")] Guid? CorrelationId,
    [property: JsonProperty(PropertyName = "$causationId")] Guid? CausationId)
{}
    