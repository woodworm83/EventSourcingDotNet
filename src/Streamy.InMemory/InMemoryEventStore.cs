using DynamicData;

namespace Streamy.InMemory;

public sealed class InMemoryEventStore<TAggregateId> : IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly SourceList<IResolvedEvent<TAggregateId>> _events = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    public IAsyncEnumerable<IResolvedEvent<TAggregateId>> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion)
        => _events.Items
            .ToAsyncEnumerable();

    public async Task<AggregateVersion> AppendEventsAsync(TAggregateId aggregateId, IEnumerable<IDomainEvent> events, AggregateVersion expectedVersion)
    {
        await _semaphore.WaitAsync();
        try
        {
            return AppendEventsUnsafe(aggregateId, events, expectedVersion);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private AggregateVersion AppendEventsUnsafe(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion expectedVersion)
    {
        var streamPosition = (ulong) _events.Count;
        var version = expectedVersion.Version;
        
        foreach (var @event in events)
        {
            _events.Add(
                new ResolvedEvent(
                    aggregateId, 
                    new AggregateVersion(++version),
                    new StreamPosition(streamPosition++),
                    @event,
                    DateTime.UtcNow));
        }

        return expectedVersion;
    }

    private readonly record struct ResolvedEvent(
            TAggregateId AggregateId,
            AggregateVersion AggregateVersion,
            StreamPosition StreamPosition,
            IDomainEvent Event,
            DateTime Timestamp)
        : IResolvedEvent<TAggregateId>;
}