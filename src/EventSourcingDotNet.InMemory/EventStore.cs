namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStore<TAggregateId> : IEventStore<TAggregateId>
    where TAggregateId : IAggregateId, IEquatable<TAggregateId>
{
    private readonly IInMemoryEventStream _eventStream;
    private readonly IEventReader _eventReader;

    public InMemoryEventStore(IInMemoryEventStream eventStream)
    {
        _eventStream = eventStream;
        _eventReader = new EventReader(eventStream);
    }

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion)
        => _eventReader.ByAggregate(aggregateId)
            .SkipWhile(resolvedEvent => resolvedEvent.AggregateVersion.Version < fromVersion.Version);

    public async ValueTask<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        => await _eventStream.AppendEventsAsync(aggregateId, events, expectedVersion, correlationId, causationId);
}