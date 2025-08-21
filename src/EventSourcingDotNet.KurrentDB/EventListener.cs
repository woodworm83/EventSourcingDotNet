using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using KurrentDB.Client;

namespace EventSourcingDotNet.KurrentDB;

internal sealed class EventListener : IEventListener, IAsyncDisposable
{
    private readonly IEventSerializer _eventSerializer;
    private readonly KurrentDBClient _client;

    public EventListener(
        IEventSerializer eventSerializer,
        KurrentDBClient client)
    {
        _eventSerializer = eventSerializer;
        _client = client;
    }

    public IObservable<ResolvedEvent> ByAggregate<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => Observable.Create<ResolvedEvent>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetAggregateStreamName(aggregateId),
                fromStreamPosition,
                false,
                observer));

    public IObservable<ResolvedEvent> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => Observable.Create<ResolvedEvent>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
                fromStreamPosition,
                true,
                observer));

    public IObservable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => Observable.Create<ResolvedEvent>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByEventStreamName<TEvent>(),
                fromStreamPosition,
                true,
                observer));

    private async Task<IDisposable> SubscribeAsync(
        string streamName,
        StreamPosition fromStreamPosition,
        bool resolveLinkTos,
        IObserver<ResolvedEvent> observer)
    {
        var listener = new Listener(observer, _eventSerializer);

        return await _client.SubscribeToStreamAsync(
            streamName,
            GetFromStream(fromStreamPosition),
            listener.EventAppeared,
            resolveLinkTos,
            listener.SubscriptionDropped
        ).ConfigureAwait(false);
    }
    
    private static FromStream GetFromStream(StreamPosition fromStreamPosition)
        => fromStreamPosition.Position switch
        {
            0 => FromStream.Start,
            ulong.MaxValue => FromStream.End,
            var position => FromStream.After(new global::KurrentDB.Client.StreamPosition(position - 1)),
        };

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync().ConfigureAwait(false);
    }

    private sealed class Listener
    {
        private readonly IObserver<ResolvedEvent> _observer;
        private readonly IEventSerializer _eventSerializer;

        public Listener(
            IObserver<ResolvedEvent> observer,
            IEventSerializer eventSerializer)
        {
            _observer = observer;
            _eventSerializer = eventSerializer;
        }

        [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed")]
        public async Task EventAppeared(
            StreamSubscription subscription,
            global::KurrentDB.Client.ResolvedEvent resolvedEvent,
            CancellationToken cancellationToken)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (resolvedEvent.Event is null) return;
            
            _observer.OnNext(await _eventSerializer.DeserializeAsync(resolvedEvent).ConfigureAwait(false));
        }

        [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed",
            Justification = "Parameters used to match callback")]
        public void SubscriptionDropped(
            StreamSubscription subscription,
            SubscriptionDroppedReason reason,
            Exception? exception)
        {
            switch (reason, exception)
            {
                case (_, not null):
                    _observer.OnError(exception);
                    break;

                case (SubscriptionDroppedReason.Disposed, _):
                    _observer.OnCompleted();
                    break;

                default:
                    _observer.OnError(new ApplicationException($"Subscription was dropped: {reason}"));
                    break;
            }
        }
    }
}