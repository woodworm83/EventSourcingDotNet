namespace Streamy;

public interface IEventPublisher<TAggregateId> 
    where TAggregateId : IAggregateId
{
    IObservable<IResolvedEvent<TAggregateId>> Listen(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default);

    IObservable<IResolvedEvent<TAggregateId>> Listen(
        StreamPosition fromStreamPosition = default);

    IObservable<IResolvedEvent<TAggregateId>> Listen<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent<TAggregateId>;
}