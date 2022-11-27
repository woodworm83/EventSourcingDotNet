namespace EventSourcingDotNet;

public interface ISnapshotStore<TAggregateId, TState>
    where TAggregateId : IAggregateId
    where TState : new()
{
    Task<Aggregate<TAggregateId, TState>?> GetLatestSnapshotAsync(
        TAggregateId aggregateId);
}