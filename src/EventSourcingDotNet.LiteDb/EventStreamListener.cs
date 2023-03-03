using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using LiteDB;

namespace EventSourcingDotNet.LiteDb;

internal sealed class EventStreamListener : IDisposable
{
    private readonly IDisposable _subscription;
    private readonly ILiteCollection<EventRecord> _collection;
    private readonly Expression<Func<EventRecord, bool>> _filter;

    private long _streamPosition;

    public EventStreamListener(
        IObserver<EventRecord> observer,
        IObservable<Unit> eventsAdded,
        ILiteCollection<EventRecord> collection,
        Expression<Func<EventRecord, bool>> filter,
        long streamPosition)
    {
        _streamPosition = streamPosition;
        _collection = collection;
        _filter = filter;

        _subscription = eventsAdded
            .StartWith(Unit.Default)
            .SelectMany(_ => ReadNewEvents())
            .Subscribe(observer);
    }

    private IEnumerable<EventRecord> ReadNewEvents()
    {
        foreach (var @event in GetEvents())
        {
            _streamPosition = @event.StreamPosition;
            yield return @event;
        }
    }

    private IEnumerable<EventRecord> GetEvents()
        => _collection.Query()
            .Where(_filter)
            .Where(@event => @event.StreamPosition >= _streamPosition)
            .OrderBy(@event => @event.StreamPosition)
            .ToEnumerable();

    public void Dispose()
    {
        _subscription.Dispose();
    }
}