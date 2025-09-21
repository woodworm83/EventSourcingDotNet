using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;

namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStream : IInMemoryEventStream
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly SourceList<ImmutableArray<IResolvedEvent>> _events = new();
    private readonly IScheduler? _scheduler;

    public InMemoryEventStream(IScheduler? scheduler = null)
    {
        _scheduler = scheduler;
    }

    public async ValueTask<AggregateVersion> AppendEventsAsync<TAggregateId>(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);

        try
        {
            CheckVersion(aggregateId, expectedVersion);
            correlationId ??= new();

            return AppendEventsUnsafe(
                aggregateId,
                events,
                expectedVersion,
                correlationId.Value,
                causationId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CheckVersion<TAggregateId>(TAggregateId aggregateId, AggregateVersion expectedVersion)
        where TAggregateId : IAggregateId
    {
        var actualVersion = GetAggregateVersion(aggregateId);

        if (actualVersion != expectedVersion)
        {
            throw new OptimisticConcurrencyException(expectedVersion, actualVersion);
        }
    }

    private AggregateVersion GetAggregateVersion<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId
        => _events
            .Items
            .SelectMany(events => events)
            .Where(resolvedEvent => resolvedEvent.StreamName.Equals(GetStreamName(aggregateId)))
            .Select(resolvedEvent => resolvedEvent.AggregateVersion)
            .LastOrDefault();

    private AggregateVersion AppendEventsUnsafe<TAggregateId>(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion currentVersion,
        CorrelationId correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId
    {
        var streamPosition = (ulong)_events.Count;

        _events.Add(
            events
                .Select(@event => CreateResolvedEvent(
                    aggregateId,
                    ref currentVersion,
                    correlationId,
                    causationId,
                    @event,
                    ref streamPosition))
                .ToImmutableArray<IResolvedEvent>());

        return currentVersion;
    }

    private static ResolvedEvent<TAggregateId> CreateResolvedEvent<TAggregateId>(
        TAggregateId aggregateId,
        ref AggregateVersion currentVersion,
        CorrelationId correlationId,
        CausationId? causationId,
        IDomainEvent @event,
        ref ulong streamPosition)
        where TAggregateId : IAggregateId
        => new(
            new(Guid.NewGuid()),
            $"{TAggregateId.AggregateName}-{aggregateId.AsString()}",
            aggregateId,
            ++currentVersion,
            new(streamPosition++),
            @event,
            DateTime.UtcNow,
            correlationId,
            causationId);

    private static string GetStreamName<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId
        => $"{TAggregateId.AggregateName}-{aggregateId.AsString()}";

    public IAsyncEnumerable<IResolvedEvent> ReadEventsAsync(StreamPosition fromStreamPosition = default)
        => _events
            .Items
            .SelectMany(events => events)
            .SkipWhile(resolvedEvent => resolvedEvent.StreamPosition.Position < fromStreamPosition.Position)
            .ToAsyncEnumerable();

    public IObservable<IResolvedEvent> Listen(StreamPosition fromStreamPosition = default)
        => Observable
            .Create<ImmutableArray<IResolvedEvent>>(observer => _events
                .Connect()
                .OnItemAdded(observer.OnNext)
                .Subscribe())
            .ObserveOn(_scheduler ?? new EventLoopScheduler())
            .SelectMany(events => events)
            .SkipWhile(resolvedEvent => resolvedEvent.StreamPosition.Position < fromStreamPosition.Position);
}