namespace EventSourcingDotNet;

public interface IResolvedEvent
{
    StreamPosition StreamPosition { get; }
    IDomainEvent Event { get; }
    DateTime Timestamp { get; }
    CorrelationId? CorrelationId { get; }
    CausationId? CausationId { get; }
}

public readonly record struct ResolvedEvent<TAggregateId>(
        EventId Id,
        TAggregateId AggregateId,
        AggregateVersion AggregateVersion,
        StreamPosition StreamPosition,
        IDomainEvent<TAggregateId> Event,
        DateTime Timestamp,
        CorrelationId? CorrelationId,
        CausationId? CausationId)
    : IResolvedEvent
    where TAggregateId : IAggregateId
{
    IDomainEvent IResolvedEvent.Event => Event;
}

public readonly record struct EventId(Guid Id);