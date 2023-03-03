namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshotStore<TAggregateId, TState> : ISnapshotStore<TAggregateId, TState> 
    where TAggregateId : IAggregateId 
    where TState : IAggregateState<TState>, new()
{
    private readonly Dictionary<TAggregateId, Aggregate<TAggregateId, TState>> _snapshots = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    public Task<Aggregate<TAggregateId, TState>?> GetAsync(TAggregateId aggregateId)
        => Task.FromResult(_snapshots.GetValueOrDefault(aggregateId));

    public async Task SetAsync(Aggregate<TAggregateId, TState> aggregate)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!IsNewer(aggregate)) return;

            _snapshots[aggregate.Id] = aggregate;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private bool IsNewer(Aggregate<TAggregateId, TState> aggregate)
    {
        if (!_snapshots.TryGetValue(aggregate.Id, out var current)) return true;
        
        return current.Version.Version >= aggregate.Version.Version;
    }
}