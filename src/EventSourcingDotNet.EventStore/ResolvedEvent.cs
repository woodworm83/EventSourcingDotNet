namespace EventSourcingDotNet.EventStore;

internal readonly record struct ResolvedEvent<TAggregateId>(
        TAggregateId AggregateId, 
        AggregateVersion AggregateVersion, 
        StreamPosition StreamPosition, 
        IDomainEvent<TAggregateId> Event, 
        DateTime Timestamp) 
    : IResolvedEvent<TAggregateId> 
    where TAggregateId : IAggregateId;