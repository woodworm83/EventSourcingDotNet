﻿using EventStore.Client;
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
        _client = new EventStoreClient(clientSettings);
    }

    public async IAsyncEnumerable<IResolvedEvent<TAggregateId>> ReadEventsAsync(TAggregateId aggregateId, AggregateVersion fromVersion)
    {
        var result = _client.ReadStreamAsync(
            Direction.Forwards,
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
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
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
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