using EventStore.Client;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventStore<TAggregateId> : IEventStore<TAggregateId>, IAsyncDisposable
    where TAggregateId : IAggregateId
{
    private readonly EventStoreClient _client;
    private readonly IEventSerializer _eventSerializer;

    public EventStore( 
        IEventSerializer eventSerializer,
        EventStoreClient eventStoreClient)
    {
        _eventSerializer = eventSerializer;
        _client = eventStoreClient;
    }

    public async IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync(TAggregateId aggregateId, AggregateVersion fromVersion)
    {
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            new global::EventStore.Client.StreamPosition(fromVersion.Version));

        if (await result.ReadState == ReadState.StreamNotFound) yield break;
        
        await foreach (var resolvedEvent in result)
        {
            yield return await _eventSerializer.DeserializeAsync<TAggregateId>(resolvedEvent);
        }
    }

    public async ValueTask<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId, 
        IEnumerable<IDomainEvent> events, 
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
    {
        var result = await _client.AppendToStreamAsync(
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            new StreamRevision(expectedVersion.Version - 1),
            await SerializeEventsAsync(aggregateId, events, correlationId, causationId).ToListAsync());

        return new AggregateVersion(result.NextExpectedStreamRevision.ToUInt64() + 1);
    }

    private async IAsyncEnumerable<EventData> SerializeEventsAsync(
        TAggregateId aggregateId, 
        IEnumerable<IDomainEvent> events,
        CorrelationId? correlationId,
        CausationId? causationId)
    {
        foreach (var @event in events)
        {
            yield return await _eventSerializer.SerializeAsync(aggregateId, @event, correlationId, causationId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}