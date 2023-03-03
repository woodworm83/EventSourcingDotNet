namespace EventSourcingDotNet;

public interface ISnapshotStore<TAggregateId, TState>
    where TAggregateId : IAggregateId
    where TState : IAggregateState<TState>, new()
{
    Task<Aggregate<TAggregateId, TState>?> GetAsync(TAggregateId aggregateId);

    Task SetAsync(Aggregate<TAggregateId, TState> aggregate);
}