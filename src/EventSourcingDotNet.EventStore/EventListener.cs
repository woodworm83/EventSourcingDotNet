using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using EventStore.Client;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventListener : IEventListener, IAsyncDisposable
{
    private readonly IEventSerializer _eventSerializer;
    private readonly EventStoreClient _client;

    public EventListener(
        IEventSerializer eventSerializer,
        EventStoreClient eventStoreClient)
    {
        _eventSerializer = eventSerializer;
        _client = eventStoreClient;
    }

    public IObservable<ResolvedEvent<TAggregateId>> ByAggregateId<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => Observable.Create<ResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetAggregateStreamName(aggregateId),
                fromStreamPosition,
                false,
                _eventSerializer.DeserializeAsync<TAggregateId>,
                observer));

    public IObservable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => Observable.Create<ResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
                fromStreamPosition,
                true,
                _eventSerializer.DeserializeAsync<TAggregateId>,
                observer));

    public IObservable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => Observable.Create<ResolvedEvent>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByEventStreamName<TEvent>(),
                fromStreamPosition,
                true,
                _eventSerializer.DeserializeAsync,
                observer));

    private async Task<IDisposable> SubscribeAsync<TResolvedEvent>(
        string streamName,
        StreamPosition fromStreamPosition,
        bool resolveLinkTos,
        Func<global::EventStore.Client.ResolvedEvent, ValueTask<TResolvedEvent>> deserializeEvent,
        IObserver<TResolvedEvent> observer)
    {
        var listener = new Listener<TResolvedEvent>(observer, deserializeEvent);

        return await _client.SubscribeToStreamAsync(
            streamName,
            GetFromStream(fromStreamPosition),
            listener.EventAppeared,
            resolveLinkTos,
            listener.SubscriptionDropped
        );
    }
    
    private static FromStream GetFromStream(StreamPosition fromStreamPosition)
        => fromStreamPosition == default
            ? FromStream.Start
            : FromStream.After(new global::EventStore.Client.StreamPosition(fromStreamPosition.Position - 1));

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }

    private sealed class Listener<TResolvedEvent>
    {
        private readonly IObserver<TResolvedEvent> _observer;
        private readonly Func<global::EventStore.Client.ResolvedEvent, ValueTask<TResolvedEvent>> _deserializeEvent;

        public Listener(
            IObserver<TResolvedEvent> observer,
            Func<global::EventStore.Client.ResolvedEvent, ValueTask<TResolvedEvent>> deserializeEvent)
        {
            _observer = observer;
            _deserializeEvent = deserializeEvent;
        }

        [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed")]
        public async Task EventAppeared(
            StreamSubscription subscription,
            global::EventStore.Client.ResolvedEvent resolvedEvent,
            CancellationToken cancellationToken)
        {
            _observer.OnNext(await _deserializeEvent(resolvedEvent));
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