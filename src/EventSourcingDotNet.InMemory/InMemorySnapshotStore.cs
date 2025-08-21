namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshotStore<TAggregateId, TState> : ISnapshotStore<TAggregateId, TState> 
    where TAggregateId : IAggregateId 
    where TState : IAggregateState<TState, TAggregateId>, new()
{
    private readonly Dictionary<TAggregateId, Aggregate<TAggregateId, TState>> _snapshots = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    public Task<Aggregate<TAggregateId, TState>?> GetAsync(TAggregateId aggregateId)
        => Task.FromResult(_snapshots.GetValueOrDefault(aggregateId));

    public async Task SetAsync(Aggregate<TAggregateId, TState> aggregate)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        
        try
        {
            if (!IsNewer(aggregate)) return;

            _snapshots[aggregate.Id] = aggregate;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool IsNewer(Aggregate<TAggregateId, TState> aggregate)
    {
        if (!_snapshots.TryGetValue(aggregate.Id, out var current)) return true;
        
        return current.Version.Version >= aggregate.Version.Version;
    }
}