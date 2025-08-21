using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record ResolvedEvent(
    EventId Id,
    string StreamName,
    JToken AggregateId,
    AggregateVersion AggregateVersion,
    StreamPosition StreamPosition,
    IDomainEvent? Event,
    DateTime Timestamp,
    CorrelationId? CorrelationId,
    CausationId? CausationId)
{
    public TAggregateId? GetAggregateId<TAggregateId>()
        where TAggregateId : struct
        => AggregateId.Type == JTokenType.Object
            ? AggregateId.ToObject<TAggregateId>()
            : null;
}