namespace EventSourcingDotNet;

public interface IEventReader
{
    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByAggregate<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>;

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>()
        where TAggregateId : IAggregateId;

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByEventType<TAggregateId, TEvent>()
        where TAggregateId : IAggregateId
        where TEvent : IDomainEvent<TAggregateId>;
}