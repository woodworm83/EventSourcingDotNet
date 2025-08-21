using System.Collections.Immutable;
using JetBrains.Annotations;

namespace EventSourcingDotNet;

/// <summary>
/// Provides functionality to load an aggregate form the event stream
/// </summary>
/// <typeparam name="TAggregateId">The aggregate identifier</typeparam>
/// <typeparam name="TState">The aggregate state</typeparam>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    Task<Aggregate<TAggregateId, TState>> SaveAsync(
        Aggregate<TAggregateId, TState> aggregate,
        CorrelationId? correlationId = null,
        CausationId? causationId = null);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    sealed async Task<Aggregate<TAggregateId, TState>> UpdateAsync(
        TAggregateId id, 
        CorrelationId? correlationId, 
        CausationId? causationId, 
        params IEnumerable<IDomainEvent> events)
        => await SaveAsync(
            events.Aggregate(
                await GetByIdAsync(id), 
                (aggregate, @event) => aggregate.AddEvent(@event)),
            correlationId,
            causationId);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    sealed async Task<Aggregate<TAggregateId, TState>> UpdateAsync(
        TAggregateId id,
        CorrelationId? correlationId,
        params IEnumerable<IDomainEvent> events)
        => await UpdateAsync(id, correlationId, null, events);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    sealed async Task<Aggregate<TAggregateId, TState>> UpdateAsync(
        TAggregateId id,
        CausationId? causationId,
        params IEnumerable<IDomainEvent> events)
        => await UpdateAsync(id, null, causationId, events);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    sealed async Task<Aggregate<TAggregateId, TState>> UpdateAsync(
        TAggregateId id,
        params IEnumerable<IDomainEvent> events)
        => await UpdateAsync(id, null, null, events);
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
        
        await UpdateSnapshot(aggregate);

        return aggregate;
    }

    private async Task<Aggregate<TAggregateId, TState>?> GetSnapshotAsync(TAggregateId aggregateId)
    {
        if (_snapshotStore is null) return null;

        return await _snapshotStore.GetAsync(aggregateId);
    }

    public async Task<Aggregate<TAggregateId, TState>> SaveAsync(
        Aggregate<TAggregateId, TState> aggregate,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
    {
        var version = await _eventStore.AppendEventsAsync(
            aggregate.Id, 
            aggregate.UncommittedEvents, 
            aggregate.Version, 
            correlationId, 
            causationId);
        
        aggregate = aggregate with
        {
            UncommittedEvents = ImmutableList<IDomainEvent>.Empty,
            Version = version
        };
        
        await UpdateSnapshot(aggregate).ConfigureAwait(false);

        return aggregate;
    }

    private async Task UpdateSnapshot(Aggregate<TAggregateId, TState> aggregate)
    {
        if (_snapshotStore is not null)
        {
            await _snapshotStore.SetAsync(aggregate).ConfigureAwait(false);
        }
    }
}