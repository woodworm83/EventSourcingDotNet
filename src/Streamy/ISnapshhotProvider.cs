namespace Streamy;

public interface ISnapshotProvider<TAggregateId, TState> 
    where TState : IAggregateState<TState, TAggregateId>
{
    Task<Aggregate<TAggregateId, TState>?> GetLatestSnapshotAsync(
        TAggregateId aggregateId);
}