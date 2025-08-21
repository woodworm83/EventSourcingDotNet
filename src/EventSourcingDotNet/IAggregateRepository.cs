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
}