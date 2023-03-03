using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LiteDB;

namespace EventSourcingDotNet.LiteDb;

internal interface ILiteDbEventStream
{
    ValueTask<AggregateVersion> AppendEventsAsync(
        string aggregateName,
        string? aggregateId,
        AggregateVersion expectedVersion,
        IEnumerable<IDomainEvent> events, 
        CorrelationId? correlationId,
        CausationId? causationId);

    IAsyncEnumerable<EventRecord> ReadEventsAsync(
        string aggregateName,
        string? aggregateId,
        AggregateVersion fromVersion = default);

    IObservable<EventRecord> Listen(
        Expression<Func<EventRecord, bool>> filter,
        StreamPosition fromStreamPosition = default);
}

internal sealed class LiteDbEventStream : ILiteDbEventStream, IDisposable
{
    private readonly ILiteCollection<EventRecord> _collection;
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly BsonMapper _mapper = new();
    private readonly Subject<Unit> _eventAdded = new();

    public LiteDbEventStream(ILiteCollection<EventRecord> collection)
    {
        _collection = collection;
    }

    public async ValueTask<AggregateVersion> AppendEventsAsync(
        string aggregateName,
        string? aggregateId,
        AggregateVersion expectedVersion,
        IEnumerable<IDomainEvent> events,
        CorrelationId? correlationId,
        CausationId? causationId)
    {
        await _semaphore.WaitAsync();
        try
        {
            CheckVersion(aggregateName, aggregateId, expectedVersion);

            return UnsafeAppendEventsAsync(
                aggregateName,
                aggregateId,
                expectedVersion,
                events,
                correlationId,
                causationId);
        }
        finally
        {
            _semaphore.Release();
            _eventAdded.OnNext(Unit.Default);
        }
    }

    private AggregateVersion UnsafeAppendEventsAsync(
        string aggregateName,
        string? aggregateId,
        AggregateVersion currentVersion,
        IEnumerable<IDomainEvent> events,
        CorrelationId? correlationId,
        CausationId? causationId)
    {
        var streamPosition = _collection.LongCount();

        _collection.InsertBulk(
            events
                .Select(@event => new EventRecord(
                    new ObjectId(Guid.NewGuid().ToString()),
                    aggregateName,
                    aggregateId,
                    streamPosition++,
                    (long) (++currentVersion).Version,
                    @event.GetType().Name,
                    correlationId?.Id ?? Guid.NewGuid(),
                    causationId?.Id,
                    DateTime.UtcNow,
                    _mapper.ToDocument(@event))));

        return currentVersion;
    }

    private void CheckVersion(string aggregateName, string? aggregateId, AggregateVersion expectedVersion)
    {
        var actualVersion = GetVersion(aggregateName, aggregateId);

        if (actualVersion != expectedVersion)
        {
            throw new OptimisticConcurrencyException(expectedVersion, actualVersion);
        }
    }

    private AggregateVersion GetVersion(string aggregateName, string? aggregateId)
        => new((ulong) _collection.LongCount(
            @event => @event.AggregateName == aggregateName && @event.AggregateId == aggregateId));

    public IAsyncEnumerable<EventRecord> ReadEventsAsync(
        string aggregateName,
        string? aggregateId,
        AggregateVersion fromVersion = default)
        => ReadEvents(
                @event => @event.AggregateName == aggregateName
                          && @event.AggregateId == aggregateId
                          && @event.AggregateVersion >= (long) fromVersion.Version,
                default)
            .ToAsyncEnumerable();

    public IObservable<EventRecord> Listen(
        Expression<Func<EventRecord, bool>> filter,
        StreamPosition fromStreamPosition = default)
        => Observable.Create<EventRecord>(
            observer => new EventStreamListener(
                observer,
                _eventAdded,
                _collection,
                filter,
                (long)fromStreamPosition.Position));
    
    private IEnumerable<EventRecord> ReadEvents(
        Expression<Func<EventRecord, bool>> filter,
        long fromStreamPosition)
        => _collection.Query()
            .Where(filter)
            .Where(@event => @event.StreamPosition >= fromStreamPosition)
            .OrderBy(@event => @event.StreamPosition)
            .ToEnumerable();

    public void Dispose()
    {
        _eventAdded.Dispose();
        _semaphore.Dispose();
    }
}