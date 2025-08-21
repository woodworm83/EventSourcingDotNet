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
    /// <param name="aggregate">The aggregate to be updated</param>
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    sealed async Task<Aggregate<TAggregateId, TState>> UpdateAsync(
        Aggregate<TAggregateId, TState> aggregate,
        CorrelationId? correlationId,
        CausationId? causationId,
        params IEnumerable<IDomainEvent> events)
        => await SaveAsync(
            events.Aggregate(
                aggregate,
                (a, @event) => a.AddEvent(@event)),
            correlationId,
            causationId).ConfigureAwait(false);

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
        => await UpdateAsync(
                await GetByIdAsync(id).ConfigureAwait(false),
                correlationId,
                causationId,
                events)
            .ConfigureAwait(false);

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
        => await UpdateAsync(id, correlationId, null, events).ConfigureAwait(false);

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
        => await UpdateAsync(id, null, causationId, events).ConfigureAwait(false);

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
        => await UpdateAsync(id, null, null, events).ConfigureAwait(false);
}