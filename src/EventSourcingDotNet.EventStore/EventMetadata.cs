using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore;

internal sealed record EventMetadata(
    [property: JsonProperty(PropertyName = "$correlationId")] Guid? CorrelationId,
    [property: JsonProperty(PropertyName = "$causationId")] Guid? CausationId);
    