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

    public IAsyncEnumerable<IResolvedEvent> ByAggregate<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => ReadEventsAsync(
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            fromStreamPosition);

    public IAsyncEnumerable<IResolvedEvent> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => ReadEventsAsync(
            StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
            fromStreamPosition,
            resolveLinkTos: true);

    public IAsyncEnumerable<IResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => ReadEventsAsync(
            StreamNamingConvention.GetByEventStreamName<TEvent>(),
            fromStreamPosition,
            resolveLinkTos: true);

    private static global::KurrentDB.Client.StreamPosition GetRevision(StreamPosition streamPosition)
        => global::KurrentDB.Client.StreamPosition.FromStreamRevision(streamPosition.Position);

    private async IAsyncEnumerable<IResolvedEvent> ReadEventsAsync(
        string streamName,
        StreamPosition fromStreamPosition,
        bool resolveLinkTos = false)
    {
        if (_client.ReadStreamAsync(
                Direction.Forwards,
                streamName,
                GetRevision(fromStreamPosition),
                resolveLinkTos: resolveLinkTos)
            is not { } result)
        {
            yield break;
        }

        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound)
        {
            yield break;
        }

        await foreach (var @event in result.ConfigureAwait(false))
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (await _eventSerializer.DeserializeAsync(@event).ConfigureAwait(false) is not {} resolvedEvent) continue;

            yield return resolvedEvent;
        }
    }
}