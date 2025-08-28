using System.Collections.Immutable;

namespace EventSourcingDotNet;

internal sealed class AggregateRepository<TAggregateId, TState> : IAggregateRepository<TAggregateId, TState>
    where TAggregateId : IAggregateId
    where TState : IAggregateState<TState, TAggregateId>, new()
{
    private readonly IEventStore<TAggregateId> _eventStore;
    private readonly ISnapshotStore<TAggregateId, TState>? _snapshotStore;

    public AggregateRepository(
        IEventStore<TAggregateId> eventStore,
        ISnapshotStore<TAggregateId, TState>? snapshotStore = null)
    {
        _eventStore = eventStore;
        _snapshotStore = snapshotStore;
    }

    public async Task<Aggregate<TAggregateId, TState>> GetByIdAsync(TAggregateId id)
    {
        using var activity = Instrumentation.ActivitySource.StartActivity();

        var aggregate = await GetSnapshotAsync(id).ConfigureAwait(false) ?? new Aggregate<TAggregateId, TState>(id);

        await foreach (var resolvedEvent in _eventStore.ReadEventsAsync(aggregate.Id, aggregate.Version)
                           .ConfigureAwait(false))
        {
            aggregate = aggregate.ApplyEvent(resolvedEvent);
        }

        return aggregate;
    }

    private async Task<Aggregate<TAggregateId, TState>?> GetSnapshotAsync(TAggregateId aggregateId)
    {
        if (_snapshotStore is null) return null;

        using var activity = Instrumentation.ActivitySource.StartActivity();

        return await _snapshotStore.GetAsync(aggregateId).ConfigureAwait(false);
    }

    public async Task<Aggregate<TAggregateId, TState>> SaveAsync(
        Aggregate<TAggregateId, TState> aggregate,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
    {
        using var activity = Instrumentation.ActivitySource.StartActivity();

        var version = await _eventStore.AppendEventsAsync(
            aggregate.Id,
            aggregate.UncommittedEvents,
            aggregate.Version,
            correlationId,
            causationId).ConfigureAwait(false);

        aggregate = aggregate with
        {
            UncommittedEvents = ImmutableList<IDomainEvent>.Empty,
            Version = version,
        };

        return aggregate;
    }
}