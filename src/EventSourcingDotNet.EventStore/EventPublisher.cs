using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using EventStore.Client;
using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventListener<TAggregateId> : IEventListener<TAggregateId>, IAsyncDisposable
    where TAggregateId : IAggregateId
{
    private readonly EventStoreClient _client;
    private readonly IEventSerializer<TAggregateId> _eventSerializer;

    public EventListener(
        IOptions<EventStoreClientSettings> options,
        IEventSerializer<TAggregateId> eventSerializer)
    {
        _eventSerializer = eventSerializer;
        ClientSettings = options.Value;

        _client = new EventStoreClient(options);
    }

    public EventStoreClientSettings ClientSettings { get; }

    public IObservable<ResolvedEvent<TAggregateId>> ByAggregateId(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        => Observable.Create<ResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetAggregateStreamName(aggregateId),
                fromStreamPosition,
                false,
                observer));

    public IObservable<ResolvedEvent<TAggregateId>> ByCategory(StreamPosition fromStreamPosition = default)
        => Observable.Create<ResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
                fromStreamPosition,
                true,
                observer));

    public IObservable<ResolvedEvent<TAggregateId>> ByEventType<TEvent>(StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent<TAggregateId>
        => Observable.Create<ResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByEventStreamName<TEvent>(),
                fromStreamPosition,
                true,
                observer));

    private async Task<IDisposable> SubscribeAsync(
        string streamName,
        StreamPosition fromStreamPosition,
        bool resolveLinkTos,
        IObserver<ResolvedEvent<TAggregateId>> observer)
    {
        var listener = new Listener(observer, _eventSerializer);
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

    private sealed class Listener
    {
        private readonly IObserver<ResolvedEvent<TAggregateId>> _observer;
        private readonly IEventSerializer<TAggregateId> _eventSerializer;

        public Listener(
            IObserver<ResolvedEvent<TAggregateId>> observer,
            IEventSerializer<TAggregateId> eventSerializer)
        {
            _observer = observer;
            _eventSerializer = eventSerializer;
        }

        [SuppressMessage("Major Code Smell", "S1172:Unused method parameters should be removed")]
        public async Task EventAppeared(
            StreamSubscription subscription,
            ResolvedEvent resolvedEvent,
            CancellationToken cancellationToken)
        {
            if (await _eventSerializer.DeserializeAsync(resolvedEvent) is { } @event)
            {
                _observer.OnNext(@event);
            }
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