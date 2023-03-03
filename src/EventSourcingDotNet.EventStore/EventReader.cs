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
            fromStreamPosition);

    public IAsyncEnumerable<ResolvedEvent> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default) 
        where TAggregateId : IAggregateId
        => ReadEventsAsync(
            StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
            fromStreamPosition);

    public IAsyncEnumerable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => ReadEventsAsync(
            StreamNamingConvention.GetByEventStreamName<TEvent>(),
            fromStreamPosition);

    private static global::EventStore.Client.StreamPosition GetRevision(StreamPosition streamPosition)
        => global::EventStore.Client.StreamPosition.FromStreamRevision(
            new StreamRevision(streamPosition.Position));
    
    private async IAsyncEnumerable<ResolvedEvent> ReadEventsAsync(
        string streamName, 
        StreamPosition fromStreamPosition)
    {
        if (_client.ReadStreamAsync(Direction.Forwards, streamName, GetRevision(fromStreamPosition)) is not { } result)
            yield break;

        if (await result.ReadState == ReadState.StreamNotFound)
            yield break;

        await foreach (var @event in result)
        {
            yield return await _eventSerializer.DeserializeAsync(@event);
        }
    }
}