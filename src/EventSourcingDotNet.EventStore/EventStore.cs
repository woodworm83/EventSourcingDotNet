using EventStore.Client;
using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventStore<TAggregateId> : IEventStore<TAggregateId>, IAsyncDisposable
    where TAggregateId : IAggregateId
{
    private readonly EventStoreClient _client;
    private readonly IEventSerializer<TAggregateId> _eventSerializer;

    public EventStore(
        IOptions<EventStoreClientSettings> clientSettings, 
        IEventSerializer<TAggregateId> eventSerializer)
    {
        _eventSerializer = eventSerializer;
        ClientSettings = clientSettings.Value;
        _client = new EventStoreClient(clientSettings);
    }
    
    public EventStoreClientSettings ClientSettings { get; }

    public async IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync(TAggregateId aggregateId, AggregateVersion fromVersion)
    {
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            new global::EventStore.Client.StreamPosition(fromVersion.Version));

        if (await result.ReadState == ReadState.StreamNotFound) yield break;
        
        await foreach (var resolvedEvent in result)
        {
            if (await _eventSerializer.DeserializeAsync(resolvedEvent) is not { } @event) continue;

            yield return @event;
        }
    }

    public async Task<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId, 
        IEnumerable<IDomainEvent<TAggregateId>> events, 
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
        IEnumerable<IDomainEvent<TAggregateId>> events,
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