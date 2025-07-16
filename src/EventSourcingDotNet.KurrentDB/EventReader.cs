using KurrentDB.Client;

namespace EventSourcingDotNet.KurrentDB;

internal sealed class EventReader : IEventReader
{
    private readonly KurrentDBClient _client;
    private readonly IEventSerializer _eventSerializer;

    public EventReader(IEventSerializer eventSerializer, KurrentDBClient client)
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
            fromStreamPosition,
            resolveLinkTos: true);

    public IAsyncEnumerable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => ReadEventsAsync(
            StreamNamingConvention.GetByEventStreamName<TEvent>(),
            fromStreamPosition,
            resolveLinkTos: true);

    private static global::KurrentDB.Client.StreamPosition GetRevision(StreamPosition streamPosition)
        => global::KurrentDB.Client.StreamPosition.FromStreamRevision(streamPosition.Position);
    
    private async IAsyncEnumerable<ResolvedEvent> ReadEventsAsync(
        string streamName, 
        StreamPosition fromStreamPosition,
        bool resolveLinkTos = false)
    {
        if (_client.ReadStreamAsync(Direction.Forwards, streamName, GetRevision(fromStreamPosition), resolveLinkTos: resolveLinkTos) is not { } result)
            yield break;

        if (await result.ReadState == ReadState.StreamNotFound)
            yield break;

        await foreach (var @event in result)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (@event.Event is null) continue;
            
            yield return await _eventSerializer.DeserializeAsync(@event);
        }
    }
}