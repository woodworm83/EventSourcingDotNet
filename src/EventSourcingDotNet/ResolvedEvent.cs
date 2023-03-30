using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet;

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
        => AggregateId.Value<TAggregateId?>();
}

public readonly record struct EventId(Guid Id);