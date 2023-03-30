using EventStore.Client;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventReader : IEventReader
{
    private readonly EventStoreClient _client;
    private readonly IEventSerializer _eventSerializer;

    public EventReader(IEventSerializer eventSerializer, EventStoreClient client)
    {
        _eventSerializer = eventSerializer;
        _client = client;
    }

    public IAsyncEnumerable<ResolvedEvent> ByAggregate<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => ReadEventsAsync(
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            fromStreamPosition,
            _eventSerializer.DeserializeAsync);

    public IAsyncEnumerable<ResolvedEvent> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default) 
        where TAggregateId : IAggregateId
        => ReadEventsAsync(
            StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
            fromStreamPosition,
            _eventSerializer.DeserializeAsync,
            resolveLinkTos: true);

    public IAsyncEnumerable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => ReadEventsAsync(
            StreamNamingConvention.GetByEventStreamName<TEvent>(),
            fromStreamPosition,
            _eventSerializer.DeserializeAsync,
            resolveLinkTos: true);

    private static global::EventStore.Client.StreamPosition GetRevision(StreamPosition streamPosition)
        => global::EventStore.Client.StreamPosition.FromStreamRevision(
            new StreamRevision(streamPosition.Position));
    
    private async IAsyncEnumerable<TResolvedEvent> ReadEventsAsync<TResolvedEvent>(
        string streamName, 
        StreamPosition fromStreamPosition,
        Func<global::EventStore.Client.ResolvedEvent, ValueTask<TResolvedEvent>> deserializeEvent,
        bool resolveLinkTos = false)
    {
        if (_client.ReadStreamAsync(Direction.Forwards, streamName, GetRevision(fromStreamPosition), resolveLinkTos: resolveLinkTos) is not { } result)
            yield break;

        if (await result.ReadState == ReadState.StreamNotFound)
            yield break;

        await foreach (var @event in result)
        {
            yield return await deserializeEvent(@event);
        }
    }
}