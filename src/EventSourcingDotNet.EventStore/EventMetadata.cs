using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet.EventStore;

internal record EventMetadata(JToken AggregateId, [property: JsonProperty(PropertyName = "$correlationId")]
    Guid? CorrelationId,
    [property: JsonProperty(PropertyName = "$causationId")]
    Guid? CausationId);