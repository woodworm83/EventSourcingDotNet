namespace EventSourcingDotNet;

public interface IEventListener<TAggregateId> 
    where TAggregateId : IAggregateId
{
    IObservable<ResolvedEvent<TAggregateId>> ByAggregateId(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default);

    IObservable<ResolvedEvent<TAggregateId>> ByCategory(
        StreamPosition fromStreamPosition = default);

    IObservable<ResolvedEvent<TAggregateId>> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent<TAggregateId>;
}