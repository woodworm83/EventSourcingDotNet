using KurrentDB.Client;

namespace EventSourcingDotNet.KurrentDB;

internal sealed class EventStore<TAggregateId> : IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly KurrentDBClient _client;
    private readonly IEventSerializer _eventSerializer;

    public EventStore(
        IEventSerializer eventSerializer,
        KurrentDBClient client)
    {
        _eventSerializer = eventSerializer;
        _client = client;
    }

    public async IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion)
    {
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            new(fromVersion.Version));

        if (await result.ReadState.ConfigureAwait(false) == ReadState.StreamNotFound) yield break;

        await foreach (var serializedEvent in result.ConfigureAwait(false))
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (await _eventSerializer.DeserializeAsync(serializedEvent).ConfigureAwait(false)
                is not ResolvedEvent<TAggregateId> resolvedEvent)
            {
                continue;
            }

            yield return resolvedEvent;
        }
    }

    public async ValueTask<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent<TAggregateId>> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
    {
        var result = await _client
            .AppendToStreamAsync(
                StreamNamingConvention.GetAggregateStreamName(aggregateId),
                StreamState.StreamRevision(expectedVersion.Version - 1),
                await SerializeEventsAsync(aggregateId, events, correlationId, causationId)
                    .ToListAsync()
                    .ConfigureAwait(false))
            .ConfigureAwait(false);

        return new AggregateVersion((uint)(result.NextExpectedStreamState.ToInt64() + 1));
    }

    private async IAsyncEnumerable<EventData> SerializeEventsAsync(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent<TAggregateId>> events,
        CorrelationId? correlationId,
        CausationId? causationId)
    {
        foreach (var @event in events)
        {
            yield return await _eventSerializer
                .SerializeAsync(aggregateId, @event, correlationId, causationId)
                .ConfigureAwait(false);
        }
    }
}