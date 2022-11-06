namespace Streamy;

public interface IAggregateRepository<TAggregateId, TState>
    where TState : IAggregateState<TState, TAggregateId>
{
    Task<Aggregate<TAggregateId, TState>> GetById(TAggregateId id);
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

    public async Task<Aggregate<TAggregateId, TState>> GetById(TAggregateId id)
    {
        var aggregate = await GetSnapshot(id) ?? new Aggregate<TAggregateId, TState>(id);

        await foreach (var resolvedEvent in _eventStore.ReadEvents(aggregate.Id, aggregate.Version))
        {
            aggregate = ApplyEvent(resolvedEvent, aggregate);
        }

        return aggregate;
    }

    private async Task<Aggregate<TAggregateId, TState>?> GetSnapshot(TAggregateId aggregateId)
    {
        if (_snapshotProvider is null) return null;

        return await _snapshotProvider.GetLatestSnapshot(aggregateId);
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
}