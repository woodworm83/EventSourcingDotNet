namespace Streamy;

public interface ISnapshotProvider<TAggregateId, TState>
    where TAggregateId : IAggregateId
    where TState : new()
{
    Task<Aggregate<TAggregateId, TState>?> GetLatestSnapshotAsync(
        TAggregateId aggregateId);
}