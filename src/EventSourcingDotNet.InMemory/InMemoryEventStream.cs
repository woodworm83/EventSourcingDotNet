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
    private readonly SourceList<IResolvedEvent> _events = new();
    
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
            return AppendEventsUnsafe(aggregateId, events, expectedVersion, correlationId ?? new CorrelationId(), causationId);
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
            .OfType<ResolvedEvent<TAggregateId>>()
            .Where(x => x.AggregateId.Equals(aggregateId))
            .Select(x => x.AggregateVersion)
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

        foreach (var @event in events)
        {
            _events.Add(
                new ResolvedEvent<TAggregateId>(
                    new EventId(Guid.NewGuid()),
                    aggregateId,
                    ++currentVersion,
                    new StreamPosition(streamPosition++),
                    @event,
                    DateTime.UtcNow,
                    correlationId,
                    causationId));
        }

        return currentVersion;
    }


    public IAsyncEnumerable<IResolvedEvent> ReadEventsAsync()
        => _events.Items.ToAsyncEnumerable();

    public IObservable<IResolvedEvent> Listen()
        => Observable.Create<IResolvedEvent>(
            observer => _events.Connect()
                .OnItemAdded(observer.OnNext)
                .Subscribe());
}