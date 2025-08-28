namespace EventSourcingDotNet;

public interface ISnapshotStore<TAggregateId, TState>
    where TAggregateId : IAggregateId
    where TState : IAggregateState<TState, TAggregateId>, new()
{
    Task<Aggregate<TAggregateId, TState>?> GetAsync(TAggregateId aggregateId);
}