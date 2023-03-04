using System.Collections.Immutable;

namespace EventSourcingDotNet;

/// <summary>
/// Provides functionality to load an aggregate form the event stream
/// </summary>
/// <typeparam name="TAggregateId">The aggregate identifier</typeparam>
/// <typeparam name="TState">The aggregate state</typeparam>
public interface IAggregateRepository<TAggregateId, TState> 
    where TAggregateId : IAggregateId
    where TState : IAggregateState<TState, TAggregateId>, new()
{
    /// <summary>
    /// Creates an aggregate and replays the events from the event store
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <returns>The aggregate with all stored events replayed</returns>
    Task<Aggregate<TAggregateId, TState>> GetByIdAsync(TAggregateId id);

    /// <summary>
    /// Save uncommitted events of an aggregate to an event store
    /// </summary>
    /// <param name="aggregate">The aggregate with zero to many uncommitted events</param>
    /// <returns>The aggregate with Version updated to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that there was any other change storing new events to the stream.
    /// </exception>
    Task<Aggregate<TAggregateId, TState>> SaveAsync(Aggregate<TAggregateId, TState> aggregate);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="events">The events to be appended to the event store</param>
    sealed async Task UpdateAsync(TAggregateId id, params IDomainEvent[] events)
        => await SaveAsync(
            events.Aggregate(
                await GetByIdAsync(id), 
                (aggregate, @event) => aggregate.AddEvent(@event)));
}

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
        var aggregate = await GetSnapshotAsync(id) ?? new Aggregate<TAggregateId, TState>(id);

        await foreach (var resolvedEvent in _eventStore.ReadEventsAsync(aggregate.Id, aggregate.Version))
        {
            aggregate = aggregate.ApplyEvent(resolvedEvent);
        }

        return aggregate;
    }

    private async Task<Aggregate<TAggregateId, TState>?> GetSnapshotAsync(TAggregateId aggregateId)
    {
        if (_snapshotStore is null) return null;

        return await _snapshotStore.GetAsync(aggregateId);
    }

    public async Task<Aggregate<TAggregateId, TState>> SaveAsync(Aggregate<TAggregateId, TState> aggregate)
    {
        var version = await _eventStore.AppendEventsAsync(aggregate.Id, aggregate.UncommittedEvents, aggregate.Version);
        
        return aggregate with
        {
            UncommittedEvents = ImmutableList<IDomainEvent>.Empty,
            Version = version
        };
    }
}