using System.Text;
using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.EventStore;

internal interface IEventSerializer
{
    ValueTask<EventData> SerializeAsync<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event,
        CorrelationId? correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId;

    ValueTask<ResolvedEvent> DeserializeAsync(global::EventStore.Client.ResolvedEvent resolvedEvent);
}

internal sealed class EventSerializer : IEventSerializer
{
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly IJsonSerializerSettingsFactory _serializerSettingsFactory;

    public EventSerializer(
        IEventTypeResolver eventTypeResolver,
        IJsonSerializerSettingsFactory serializerSettingsFactory)
    {
        _eventTypeResolver = eventTypeResolver;
        _serializerSettingsFactory = serializerSettingsFactory;
    }

    public async ValueTask<EventData> SerializeAsync<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event,
        CorrelationId? correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId
        => new(
            Uuid.NewUuid(),
            @event.GetType().Name,
            await SerializeEventData(aggregateId, @event),
            SerializeEventMetadata(correlationId?.Id, causationId?.Id));

    private async ValueTask<ReadOnlyMemory<byte>> SerializeEventData<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event)
        where TAggregateId : IAggregateId
        => Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                @event,
                await _serializerSettingsFactory.CreateForSerializationAsync(
                    @event.GetType(),
                    StreamNamingConvention.GetAggregateStreamName(aggregateId))));

    private static ReadOnlyMemory<byte>? SerializeEventMetadata(
        Guid? correlationId,
        Guid? causationId)
        => correlationId.HasValue || causationId.HasValue
            ? Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new EventMetadata(correlationId, causationId),
                    new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()},
                        NullValueHandling = NullValueHandling.Ignore
                    }))
            : null;

    public async ValueTask<ResolvedEvent> DeserializeAsync(global::EventStore.Client.ResolvedEvent resolvedEvent)
    {
        var metadata = DeserializeEventMetadata(resolvedEvent.Event);
        var @event = await DeserializeEventDataAsync(resolvedEvent.Event.EventStreamId, resolvedEvent.Event);

        return new ResolvedEvent(
            new EventId(resolvedEvent.Event.EventId.ToGuid()),
            resolvedEvent.Event.EventStreamId,
            new AggregateVersion(resolvedEvent.Event.EventNumber.ToUInt64() + 1),
            new StreamPosition(resolvedEvent.OriginalEvent.EventNumber.ToUInt64()),
            @event,
            resolvedEvent.Event.Created,
            metadata?.CorrelationId is { } correlationId
                ? new CorrelationId(correlationId)
                : null,
            metadata?.CausationId is { } causationId
                ? new CausationId(causationId)
                : null);
    }

    private static EventMetadata? DeserializeEventMetadata(EventRecord eventRecord)
        => JsonConvert.DeserializeObject<EventMetadata>(
            Encoding.UTF8.GetString(eventRecord.Metadata.Span),
            new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()},
            });

    private async ValueTask<IDomainEvent?> DeserializeEventDataAsync(string streamName, EventRecord eventRecord)
    {
        if (_eventTypeResolver.GetEventType(eventRecord.EventType) is not { } eventType) return null;

        return JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(eventRecord.Data.Span),
                eventType,
                await _serializerSettingsFactory.CreateForDeserializationAsync(streamName))
            as IDomainEvent;
    }
}