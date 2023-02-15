using System.Reactive.Linq;

namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStore<TAggregateId> : IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly IInMemoryEventStream _eventStream;

    public InMemoryEventStore(IInMemoryEventStream eventStream)
    {
        _eventStream = eventStream;
    }

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion)
        => _eventStream.ReadEventsAsync()
            .OfType<ResolvedEvent<TAggregateId>>();

    public async Task<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent<TAggregateId>> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        => await _eventStream.AppendEventsAsync(aggregateId, events, expectedVersion, correlationId, causationId);
    
    public IObservable<ResolvedEvent<TAggregateId>> ByAggregateId(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        => ByCategory(fromStreamPosition)
            .Where(x => x.AggregateId.Equals(aggregateId));

    public IObservable<ResolvedEvent<TAggregateId>> ByCategory(
        StreamPosition fromStreamPosition = default)
        => _eventStream.Listen()
            .SkipWhile(x => x.StreamPosition.Position < fromStreamPosition.Position)
            .OfType<ResolvedEvent<TAggregateId>>();

    public IObservable<ResolvedEvent<TAggregateId>> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default) 
        where TEvent : IDomainEvent<TAggregateId> 
        => ByCategory(fromStreamPosition)
            .Where(x => x.Event is TEvent);
}