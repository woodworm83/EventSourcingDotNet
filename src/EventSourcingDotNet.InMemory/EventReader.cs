namespace EventSourcingDotNet.InMemory;

internal sealed class EventReader : IEventReader
{
    private readonly IInMemoryEventStream _eventStream;

    public EventReader(IInMemoryEventStream eventStream)
    {
        _eventStream = eventStream;
    }

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByAggregate<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => ByCategory<TAggregateId>()
            .Where(resolvedEvent => resolvedEvent.AggregateId.Equals(aggregateId));

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>() 
        where TAggregateId : IAggregateId
        => _eventStream.ReadEventsAsync()
            .OfType<ResolvedEvent<TAggregateId>>();

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByEventType<TAggregateId, TEvent>()
        where TAggregateId : IAggregateId
        where TEvent : IDomainEvent
        => ByCategory<TAggregateId>()
            .Where(resolvedEvent => resolvedEvent.Event is TEvent);
}