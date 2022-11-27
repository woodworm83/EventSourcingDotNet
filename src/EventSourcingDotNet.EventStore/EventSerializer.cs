using System.Text;
using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.EventStore;

internal interface IEventSerializer<TAggregateId> 
    where TAggregateId : IAggregateId
{
    ValueTask<EventData> SerializeAsync(TAggregateId aggregateId, IDomainEvent<TAggregateId> @event);

    ValueTask<IResolvedEvent<TAggregateId>?> DeserializeAsync(ResolvedEvent resolvedEvent);
}

internal sealed record EventMetadata<TAggregateId>(TAggregateId AggregateId);

internal sealed class EventSerializer<TAggregateId> : IEventSerializer<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly IEventTypeResolver<TAggregateId> _eventTypeResolver;
    private readonly IJsonSerializerSettingsFactory<TAggregateId> _serializerSettingsFactory;

    public EventSerializer(IEventTypeResolver<TAggregateId> eventTypeResolver, IJsonSerializerSettingsFactory<TAggregateId> serializerSettingsFactory)
    {
        _eventTypeResolver = eventTypeResolver;
        _serializerSettingsFactory = serializerSettingsFactory;
    }

    public async ValueTask<EventData> SerializeAsync(TAggregateId aggregateId, IDomainEvent<TAggregateId> @event)
        => new(
            Uuid.NewUuid(),
            @event.GetType().Name, 
            await SerializeEventData(aggregateId, @event), 
            SerializeEventMetadata(aggregateId));

    private async ValueTask<ReadOnlyMemory<byte>> SerializeEventData(TAggregateId aggregateId, IDomainEvent<TAggregateId> @event)
        => Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                @event, 
                await _serializerSettingsFactory.CreateForSerializationAsync(aggregateId, @event.GetType())));

    private ReadOnlyMemory<byte> SerializeEventMetadata(TAggregateId aggregateId)
        => Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                new EventMetadata<TAggregateId>(aggregateId), 
                new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
                }));

    public async ValueTask<IResolvedEvent<TAggregateId>?> DeserializeAsync(ResolvedEvent resolvedEvent)
    {
        if (DeserializeEventMetadata(resolvedEvent.Event) is not { } metadata) return null;
        if (await DeserializeEventDataAsync(metadata.AggregateId, resolvedEvent.Event) is not { } @event) return null;

        return new ResolvedEvent<TAggregateId>(
            metadata.AggregateId,
            new AggregateVersion(resolvedEvent.Event.EventNumber.ToUInt64() + 1), 
            new StreamPosition(resolvedEvent.OriginalEvent.EventNumber.ToUInt64()), 
            @event, 
            resolvedEvent.Event.Created);
    }

    private EventMetadata<TAggregateId>? DeserializeEventMetadata(EventRecord eventRecord)
        => JsonConvert.DeserializeObject<EventMetadata<TAggregateId>>(
            Encoding.UTF8.GetString(eventRecord.Metadata.Span),
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy()}
            });

    private async ValueTask<IDomainEvent<TAggregateId>?> DeserializeEventDataAsync(TAggregateId aggregateId, EventRecord eventRecord)
    {
        if (_eventTypeResolver.GetEventType(eventRecord.EventType) is not { } eventType) return null;

        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(eventRecord.Data.Span),
            eventType,
            await _serializerSettingsFactory.CreateForDeserializationAsync(aggregateId))
            as IDomainEvent<TAggregateId>;
    }
}