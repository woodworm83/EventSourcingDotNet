namespace Streamy;

public interface IEventPublisher<TAggregateId>
{
    IObservable<IResolvedEvent<TAggregateId>> Listen(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default);

    IObservable<IResolvedEvent<TAggregateId>> Listen(
        StreamPosition fromStreamPosition = default);

    IObservable<IResolvedEvent<TAggregateId>> Listen<TEvent>(
        StreamPosition fromStreamPosition = default);
}