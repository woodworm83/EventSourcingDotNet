using System.Collections.Immutable;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;

namespace EventSourcingDotNet.InMemory;

internal interface IInMemoryEventStream
{
    public ValueTask<AggregateVersion> AppendEventsAsync<TAggregateId>(TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId;

    public IAsyncEnumerable<IResolvedEvent> ReadEventsAsync();

    public IObservable<IResolvedEvent> Listen();
}

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
        await _semaphore.WaitAsync();
        try
        {
            CheckVersion(aggregateId, expectedVersion);
            correlationId ??= new CorrelationId();
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
            .OfType<ResolvedEvent<TAggregateId>>()
            .Where(resolvedEvent => resolvedEvent.AggregateId.Equals(aggregateId))
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
        var streamPosition = (ulong) _events.Count;

        _events.Add(
            events.Select(
                    @event => new ResolvedEvent<TAggregateId>(
                        new EventId(Guid.NewGuid()),
                        aggregateId,
                        ++currentVersion,
                        new StreamPosition(streamPosition++),
                        @event,
                        DateTime.UtcNow,
                        correlationId,
                        causationId))
                .Cast<IResolvedEvent>()
                .ToImmutableArray());

        return currentVersion;
    }


    public IAsyncEnumerable<IResolvedEvent> ReadEventsAsync()
        => _events.Items
            .SelectMany(events => events)
            .ToAsyncEnumerable();

    public IObservable<IResolvedEvent> Listen()
        => Observable.Create<ImmutableArray<IResolvedEvent>>(
                observer => _events.Connect()
                    .OnItemAdded(observer.OnNext)
                    .Subscribe())
            .ObserveOn(_scheduler ?? new EventLoopScheduler())
            .SelectMany(events => events);
}