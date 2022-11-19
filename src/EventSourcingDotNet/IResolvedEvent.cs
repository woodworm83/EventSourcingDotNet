namespace EventSourcingDotNet;

public interface IResolvedEvent<TAggregateId> 
    where TAggregateId : IAggregateId
{
    TAggregateId AggregateId { get; }
    AggregateVersion AggregateVersion { get; }
    StreamPosition StreamPosition { get; }
    IDomainEvent<TAggregateId> Event { get; }
    DateTime Timestamp { get; }
}