namespace EventSourcingDotNet;

public interface IEventListener 
{
    IObservable<ResolvedEvent<TAggregateId>> ByAggregateId<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>;

    IObservable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId;

    IObservable<ResolvedEvent<TAggregateId>> ByEventType<TAggregateId, TEvent>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        where TEvent : IDomainEvent;
}