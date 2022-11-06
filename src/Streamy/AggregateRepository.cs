using System.Collections.Immutable;

namespace Streamy;

public interface IAggregateRepository<TAggregateId, TState>
    where TState : IAggregateState<TState, TAggregateId>
{
    Task<Aggregate<TAggregateId, TState>> GetByIdAsync(TAggregateId id);

    Task<Aggregate<TAggregateId, TState>> SaveAsync(Aggregate<TAggregateId, TState> aggregate);
}

internal sealed class AggregateRepository<TAggregateId, TState> : IAggregateRepository<TAggregateId, TState>
    where TAggregateId : IAggregateId
    where TState : IAggregateState<TState, TAggregateId>
{
    private readonly IEventStore<TAggregateId> _eventStore;
    private readonly ISnapshotProvider<TAggregateId, TState>? _snapshotProvider;

    public AggregateRepository(
        IEventStore<TAggregateId> eventStore,
        ISnapshotProvider<TAggregateId, TState>? snapshotProvider = null)
    {
        _eventStore = eventStore;
        _snapshotProvider = snapshotProvider;
    }

    public async Task<Aggregate<TAggregateId, TState>> GetByIdAsync(TAggregateId id)
    {
        var aggregate = await GetSnapshotAsync(id) ?? new Aggregate<TAggregateId, TState>(id);

        await foreach (var resolvedEvent in _eventStore.ReadEventsAsync(aggregate.Id, aggregate.Version))
        {
            aggregate = ApplyEvent(resolvedEvent, aggregate);
        }

        return aggregate;
    }

    private async Task<Aggregate<TAggregateId, TState>?> GetSnapshotAsync(TAggregateId aggregateId)
    {
        if (_snapshotProvider is null) return null;

        return await _snapshotProvider.GetLatestSnapshotAsync(aggregateId);
    }

    private static Aggregate<TAggregateId, TState> ApplyEvent(IResolvedEvent<TAggregateId> resolvedEvent,
        Aggregate<TAggregateId, TState> aggregate)
    {
        aggregate = aggregate with {Version = resolvedEvent.AggregateVersion};

        if (resolvedEvent.Event is IDomainEvent<TState> @event)
        {
            aggregate = aggregate with
            {
                State = @event.Apply(aggregate.State),
                Version = resolvedEvent.AggregateVersion
            };
        }

        return aggregate;
    }

    public async Task<Aggregate<TAggregateId, TState>> SaveAsync(Aggregate<TAggregateId, TState> aggregate)
    {
        var version = await _eventStore.AppendEventsAsync(aggregate.Id, aggregate.UncommittedEvents, aggregate.Version);
        
        return aggregate with
        {
            UncommittedEvents = ImmutableList<IDomainEvent<TState>>.Empty,
            Version = version
        };
    }
}