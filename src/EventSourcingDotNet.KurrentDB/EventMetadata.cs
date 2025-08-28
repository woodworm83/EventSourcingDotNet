using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet.KurrentDB;

internal record EventMetadata(
    JToken AggregateId,
    [property: JsonProperty(PropertyName = "$correlationId", DefaultValueHandling = DefaultValueHandling.Ignore)]
    Guid? CorrelationId,
    [property: JsonProperty(PropertyName = "$causationId", DefaultValueHandling = DefaultValueHandling.Ignore)]
    Guid? CausationId);