using System.Reactive.Linq;
using EventStore.Client;
using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventPublisher<TAggregateId> : IEventPublisher<TAggregateId>, IAsyncDisposable
    where TAggregateId : IAggregateId
{
    private readonly EventStoreClient _client;
    private readonly IEventSerializer<TAggregateId> _eventSerializer;
    
    public EventPublisher(
        IOptions<EventStoreClientSettings> options,
        IEventSerializer<TAggregateId> eventSerializer)
    {
        _eventSerializer = eventSerializer;
        ClientSettings = options.Value;
        
        _client = new EventStoreClient(options);
    }
    
    public EventStoreClientSettings ClientSettings { get; }

    public IObservable<IResolvedEvent<TAggregateId>> Listen(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        => Observable.Create<IResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetAggregateStreamName(aggregateId),
                fromStreamPosition,
                false,
                observer));

    public IObservable<IResolvedEvent<TAggregateId>> Listen(StreamPosition fromStreamPosition = default)
        => Observable.Create<IResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
                fromStreamPosition,
                true,
                observer));

    public IObservable<IResolvedEvent<TAggregateId>> Listen<TEvent>(StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent<TAggregateId>
        => Observable.Create<IResolvedEvent<TAggregateId>>(
            observer => SubscribeAsync(
                StreamNamingConvention.GetByEventStreamName<TAggregateId, TEvent>(),
                fromStreamPosition,
                true,
                observer));

    private async Task<IDisposable> SubscribeAsync(
        string streamName,
        StreamPosition fromStreamPosition,
        bool resolveLinkTos,
        IObserver<IResolvedEvent<TAggregateId>> observer)
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
        private readonly IObserver<IResolvedEvent<TAggregateId>> _observer;
        private readonly IEventSerializer<TAggregateId> _eventSerializer;

        public Listener(
            IObserver<IResolvedEvent<TAggregateId>> observer,
            IEventSerializer<TAggregateId> eventSerializer)
        {
            _observer = observer;
            _eventSerializer = eventSerializer;
        }

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