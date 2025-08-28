using System.Collections.Immutable;
using Microsoft.Extensions.Hosting;

namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshotStore<TAggregateId, TState>(IEventListener eventListener)
    : BackgroundService, ISnapshotStore<TAggregateId, TState>
    where TAggregateId : struct, IAggregateId
    where TState : IAggregateState<TState, TAggregateId>, new()
{
    private State _state = State.InitialState;

    public Task<Aggregate<TAggregateId, TState>?> GetAsync(TAggregateId aggregateId)
        => Task.FromResult(_state.Snapshots.GetValueOrDefault(aggregateId));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var resolvedEvent in eventListener
                           .ByCategory<TAggregateId>(_state.StreamPosition)
                           .ToAsyncEnumerable()
                           .WithCancellation(stoppingToken)
                           .ConfigureAwait(false))
        {
            _state = new State(
                StreamPosition: resolvedEvent.StreamPosition,
                Snapshots: ApplyEvent(_state.Snapshots, resolvedEvent));
        }
    }

    private static IImmutableDictionary<TAggregateId, Aggregate<TAggregateId, TState>> ApplyEvent(
        IImmutableDictionary<TAggregateId, Aggregate<TAggregateId, TState>> snapshots,
        ResolvedEvent resolvedEvent)
        => resolvedEvent.GetAggregateId<TAggregateId>() is { } aggregateId
            ? snapshots.SetItem(
                aggregateId,
                GetOrCreateAggregate(snapshots, aggregateId)
                    .ApplyEvent(resolvedEvent))
            : snapshots;

    private static Aggregate<TAggregateId, TState> GetOrCreateAggregate(
        IImmutableDictionary<TAggregateId, Aggregate<TAggregateId, TState>> snapshots, TAggregateId aggregateId)
        => snapshots.GetValueOrDefault(aggregateId)
           ?? new Aggregate<TAggregateId, TState>(aggregateId);

    private sealed record State(
        StreamPosition StreamPosition,
        IImmutableDictionary<TAggregateId, Aggregate<TAggregateId, TState>> Snapshots)
    {
        public static State InitialState { get; } = new(
            StreamPosition.Start,
            ImmutableDictionary<TAggregateId, Aggregate<TAggregateId, TState>>.Empty);
    }
}