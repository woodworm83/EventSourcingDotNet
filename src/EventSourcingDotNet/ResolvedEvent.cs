namespace EventSourcingDotNet;

public readonly record struct ResolvedEvent(
        EventId Id,
        string StreamName,
        AggregateVersion AggregateVersion,
        StreamPosition StreamPosition,
        IDomainEvent? Event,
        DateTime Timestamp,
        CorrelationId? CorrelationId,
        CausationId? CausationId);

public readonly record struct EventId(Guid Id);