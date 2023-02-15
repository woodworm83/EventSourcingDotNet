using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventReader : IEventReader
{
    private readonly EventStoreClient _client;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventReader(IServiceScopeFactory serviceScopeFactory, EventStoreClient client)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _client = client;
    }

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByAggregate<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => ReadEventsAsync<TAggregateId>(
            StreamNamingConvention.GetAggregateStreamName(aggregateId),
            global::EventStore.Client.StreamPosition.Start);

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>() 
        where TAggregateId : IAggregateId
        => ReadEventsAsync<TAggregateId>(
            StreamNamingConvention.GetByCategoryStreamName<TAggregateId>(),
            global::EventStore.Client.StreamPosition.Start);

    public IAsyncEnumerable<ResolvedEvent<TAggregateId>> ByEventType<TAggregateId, TEvent>()
        where TEvent : IDomainEvent<TAggregateId>
        where TAggregateId : IAggregateId
        => ReadEventsAsync<TAggregateId>(
            StreamNamingConvention.GetByEventStreamName<TEvent>(),
            global::EventStore.Client.StreamPosition.Start);
    
    private async IAsyncEnumerable<ResolvedEvent<TAggregateId>> ReadEventsAsync<TAggregateId>(string streamName, global::EventStore.Client.StreamPosition direction)
        where TAggregateId : IAggregateId
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var eventSerializer = scope.ServiceProvider.GetRequiredService<IEventSerializer<TAggregateId>>();

        if (_client.ReadStreamAsync(Direction.Forwards, streamName, direction) is not { } result)
            yield break;

        if (await result.ReadState == ReadState.StreamNotFound)
            yield break;

        await foreach (var @event in result)
        {
            if (await eventSerializer.DeserializeAsync(@event) is not { } resolvedEvent)
                continue;

            yield return resolvedEvent;
        }
    }
}