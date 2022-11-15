using EventStore.Client;
using Microsoft.Extensions.Options;

namespace Streamy.EventStore;

internal sealed class EventStore<TAggregateId> : IEventStore<TAggregateId>, IAsyncDisposable
    where TAggregateId : IAggregateId
{
    private readonly EventStoreClient _client;
    private readonly IEventSerializer<TAggregateId> _eventSerializer;
    private readonly IStreamNamingConvention<TAggregateId> _streamNamingConvention;

    public EventStore(
        IOptions<EventStoreClientSettings> clientSettings, 
        IEventSerializer<TAggregateId> eventSerializer, 
        IStreamNamingConvention<TAggregateId> streamNamingConvention)
    {
        _eventSerializer = eventSerializer;
        _streamNamingConvention = streamNamingConvention;
        _client = new EventStoreClient(clientSettings);
    }

    public async IAsyncEnumerable<IResolvedEvent<TAggregateId>> ReadEventsAsync(TAggregateId aggregateId, AggregateVersion fromVersion)
    {
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            _streamNamingConvention.GetAggregateStreamName(aggregateId),
            new global::EventStore.Client.StreamPosition(fromVersion.Version));

        if (await result.ReadState == ReadState.StreamNotFound) yield break;
        
        await foreach (var resolvedEvent in result)
        {
            if (_eventSerializer.Deserialize(resolvedEvent) is not { } @event) continue;

            yield return @event;
        }
    }

    public async Task<AggregateVersion> AppendEventsAsync(TAggregateId aggregateId, IEnumerable<IDomainEvent<TAggregateId>> events, AggregateVersion expectedVersion)
    {
        var result = await _client.AppendToStreamAsync(
            _streamNamingConvention.GetAggregateStreamName(aggregateId),
            new StreamRevision(expectedVersion.Version - 1),
            SerializeEvents(aggregateId, events));

        return new AggregateVersion(result.NextExpectedStreamRevision.ToUInt64() + 1);
    }

    private IEnumerable<EventData> SerializeEvents(TAggregateId aggregateId, IEnumerable<IDomainEvent<TAggregateId>> events)
    {
        return events
            .Select(@event => _eventSerializer.Serialize(aggregateId, @event));
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}

internal readonly record struct ResolvedEvent<TAggregateId>(
    TAggregateId AggregateId, 
    AggregateVersion AggregateVersion, 
    StreamPosition StreamPosition, 
    IDomainEvent<TAggregateId> Event, 
    DateTime Timestamp) 
    : IResolvedEvent<TAggregateId> 
    where TAggregateId : IAggregateId;