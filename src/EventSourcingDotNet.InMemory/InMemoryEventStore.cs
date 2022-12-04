using System.Reactive.Linq;
using DynamicData;

namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStore<TAggregateId> : IEventStore<TAggregateId>, IEventListener<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly SourceList<ResolvedEvent<TAggregateId>> _events = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion)
        => _events.Items
            .ToAsyncEnumerable();

    public async Task<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent<TAggregateId>> events, 
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            CheckVersion(aggregateId, expectedVersion);
            return AppendEventsUnsafe(aggregateId, events, expectedVersion, correlationId ?? new CorrelationId(), causationId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CheckVersion(TAggregateId aggregateId, AggregateVersion expectedVersion)
    {
        var actualVersion = GetAggregateVersion(aggregateId);

        if (actualVersion != expectedVersion)
        {
            throw new OptimisticConcurrencyException(expectedVersion, actualVersion);
        }
    }

    private AggregateVersion GetAggregateVersion(TAggregateId aggregateId)
        => _events
            .Items
            .Where(x => x.AggregateId.Equals(aggregateId))
            .Select(x => x.AggregateVersion)
            .LastOrDefault();

    private AggregateVersion AppendEventsUnsafe(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent<TAggregateId>> events,
        AggregateVersion currentVersion,
        CorrelationId correlationId,
        CausationId? causationId)
    {
        var streamPosition = (ulong) _events.Count;

        foreach (var @event in events)
        {
            _events.Add(
                new ResolvedEvent<TAggregateId>(
                    new EventId(Guid.NewGuid()),
                    aggregateId,
                    ++currentVersion,
                    new StreamPosition(streamPosition++),
                    @event,
                    DateTime.UtcNow,
                    correlationId,
                    causationId));
        }

        return currentVersion;
    }

    public IObservable<ResolvedEvent<TAggregateId>> ByAggregateId(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        => ByCategory(fromStreamPosition)
            .Where(x => x.AggregateId.Equals(aggregateId));

    public IObservable<ResolvedEvent<TAggregateId>> ByCategory(
        StreamPosition fromStreamPosition = default)
        => Observable.Create<ResolvedEvent<TAggregateId>>(
                observer => _events.Connect()
                    .OnItemAdded(observer.OnNext)
                    .Subscribe())
            .SkipWhile(x => x.StreamPosition.Position < fromStreamPosition.Position);

    public IObservable<ResolvedEvent<TAggregateId>> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default) 
        where TEvent : IDomainEvent<TAggregateId> 
        => ByCategory(fromStreamPosition)
            .Where(x => x.Event is TEvent);
}