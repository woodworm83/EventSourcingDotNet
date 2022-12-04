namespace EventSourcingDotNet;

public interface IEventPublisher<TAggregateId> 
    where TAggregateId : IAggregateId
{
    IObservable<ResolvedEvent<TAggregateId>> Listen(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default);

    IObservable<ResolvedEvent<TAggregateId>> Listen(
        StreamPosition fromStreamPosition = default);

    IObservable<ResolvedEvent<TAggregateId>> Listen<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent<TAggregateId>;
}