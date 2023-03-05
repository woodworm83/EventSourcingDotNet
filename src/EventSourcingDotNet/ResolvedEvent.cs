namespace EventSourcingDotNet;

public record ResolvedEvent(
    EventId Id,
    string StreamName,
    AggregateVersion AggregateVersion,
    StreamPosition StreamPosition,
    IDomainEvent? Event,
    DateTime Timestamp,
    CorrelationId? CorrelationId,
    CausationId? CausationId);

public sealed record ResolvedEvent<TAggregateId>(
        EventId Id,
        string StreamName,
        TAggregateId? AggregateId,
        AggregateVersion AggregateVersion,
        StreamPosition StreamPosition,
        IDomainEvent? Event,
        DateTime Timestamp,
        CorrelationId? CorrelationId,
        CausationId? CausationId)
    : ResolvedEvent(Id, StreamName, AggregateVersion, StreamPosition, Event, Timestamp, CorrelationId, CausationId)
    where TAggregateId : IAggregateId;

public readonly record struct EventId(Guid Id);