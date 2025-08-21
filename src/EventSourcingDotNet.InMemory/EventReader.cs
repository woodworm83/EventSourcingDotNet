namespace EventSourcingDotNet.InMemory;

internal sealed class EventReader : IEventReader
{
    private readonly IInMemoryEventStream _eventStream;

    public EventReader(IInMemoryEventStream eventStream)
    {
        _eventStream = eventStream;
    }

    public IAsyncEnumerable<ResolvedEvent> ByAggregate<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => _eventStream.ReadEventsAsync(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.StreamName.Equals(
                $"{TAggregateId.AggregateName}-{aggregateId.AsString()}"));

    public IAsyncEnumerable<ResolvedEvent> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => _eventStream.ReadEventsAsync(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.StreamName.StartsWith(
                $"{TAggregateId.AggregateName}-",
                StringComparison.OrdinalIgnoreCase));

    public IAsyncEnumerable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => _eventStream.ReadEventsAsync(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.Event is TEvent);
}