using JetBrains.Annotations;

namespace EventSourcingDotNet;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class AggregateRepositoryExtensions
{
    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="repository">The repository to update</param>
    /// <param name="aggregate">The aggregate to be updated</param>
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    public static async Task<Aggregate<TAggregateId, TState>> UpdateAsync<TAggregateId, TState>(
        this IAggregateRepository<TAggregateId, TState> repository,
        Aggregate<TAggregateId, TState> aggregate,
        CorrelationId? correlationId,
        CausationId? causationId,
        params IEnumerable<IDomainEvent> events)
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>, new()
        => await repository
            .SaveAsync(
                events.Aggregate(
                    aggregate,
                    (a, @event) => a.AddEvent(@event)),
                correlationId,
                causationId)
            .ConfigureAwait(false);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="repository">The repository to update</param>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    public static async Task<Aggregate<TAggregateId, TState>> UpdateAsync<TAggregateId, TState>(
        this IAggregateRepository<TAggregateId, TState> repository,
        TAggregateId id,
        CorrelationId? correlationId,
        CausationId? causationId,
        params IEnumerable<IDomainEvent> events)
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>, new()
        => await repository.UpdateAsync(
                await repository.GetByIdAsync(id).ConfigureAwait(false),
                correlationId,
                causationId,
                events)
            .ConfigureAwait(false);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="repository">The repository to update</param>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="correlationId">The correlation id of the process</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    public static async Task<Aggregate<TAggregateId, TState>> UpdateAsync<TAggregateId, TState>(
        this IAggregateRepository<TAggregateId, TState> repository,
        TAggregateId id,
        CorrelationId? correlationId,
        params IEnumerable<IDomainEvent> events)
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>, new()
        => await repository
            .UpdateAsync(id, correlationId, causationId: null, events)
            .ConfigureAwait(false);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="repository">The repository to update</param>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="causationId">The causation id which initiated the events</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    public static async Task<Aggregate<TAggregateId, TState>> UpdateAsync<TAggregateId, TState>(
        this IAggregateRepository<TAggregateId, TState> repository,
        TAggregateId id,
        CausationId? causationId,
        params IEnumerable<IDomainEvent> events)
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>, new()
        => await repository
            .UpdateAsync(id, correlationId: null, causationId, events)
            .ConfigureAwait(false);

    /// <summary>
    /// In-place update of an aggregate.
    /// </summary>
    /// <param name="repository">The repository to update</param>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="events">The events to be appended to the event store</param>
    /// <returns>The aggregate with updated version and uncommitted events cleared to allow additional changes without reloading.</returns>
    /// <exception cref="OptimisticConcurrencyException">
    /// Thrown when expected version of the aggregate does not match actual version of the event store.
    /// This indicates that new events were added to the stream since the aggregate was replayed.
    /// </exception>
    public static async Task<Aggregate<TAggregateId, TState>> UpdateAsync<TAggregateId, TState>(
        this IAggregateRepository<TAggregateId, TState> repository,
        TAggregateId id,
        params IEnumerable<IDomainEvent> events)
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TState, TAggregateId>, new()
        => await repository
            .UpdateAsync(id, correlationId: null, causationId: null, events)
            .ConfigureAwait(false);
}