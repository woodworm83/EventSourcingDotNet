using Microsoft.Extensions.Hosting;

namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshot<TAggregateId, TState> : BackgroundService, ISnapshotStore<TAggregateId, TState> 
    where TAggregateId : IAggregateId 
    where TState : new()
{
    private readonly Dictionary<TAggregateId, Aggregate<TAggregateId, TState>> _snapshots = new();
    private readonly IEventListener<TAggregateId> _eventListener;

    public InMemorySnapshot(IEventListener<TAggregateId> eventListener)
    {
        _eventListener = eventListener;
    }

    public Task<Aggregate<TAggregateId, TState>?> GetLatestSnapshotAsync(TAggregateId aggregateId)
        => Task.FromResult(_snapshots.GetValueOrDefault(aggregateId));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (_eventListener.ByCategory().Subscribe(HandleEvent))
        {
            var tcs = new TaskCompletionSource();
            stoppingToken.Register(tcs.SetResult);
            await tcs.Task;
        }
    }

    private void HandleEvent(ResolvedEvent<TAggregateId> resolvedEvent)
        => _snapshots[resolvedEvent.AggregateId] = GetSnapshotOrNew(resolvedEvent.AggregateId).ApplyEvent(resolvedEvent);

    private Aggregate<TAggregateId, TState> GetSnapshotOrNew(TAggregateId aggregateId) =>
        _snapshots.GetValueOrDefault(aggregateId)
        ?? new Aggregate<TAggregateId, TState>(aggregateId);
}