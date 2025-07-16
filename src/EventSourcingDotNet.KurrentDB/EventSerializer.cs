using System.Text;
using EventSourcingDotNet.Serialization.Json;
using KurrentDB.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB;

internal interface IEventSerializer
{
    ValueTask<EventData> SerializeAsync<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId;

    ValueTask<ResolvedEvent> DeserializeAsync(global::KurrentDB.Client.ResolvedEvent resolvedEvent);
}

internal sealed class EventSerializer : IEventSerializer
{
    private static readonly JsonSerializerSettings MetadataSerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
        NullValueHandling = NullValueHandling.Ignore,
        Converters = { new StringEnumConverter() }
    };

    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly IJsonSerializerSettingsFactory _serializerSettingsFactory;
    private readonly EventSerializerSettings? _settings;

    public EventSerializer(
        IEventTypeResolver eventTypeResolver,
        IJsonSerializerSettingsFactory serializerSettingsFactory,
        EventSerializerSettings? settings = null)
    {
        _eventTypeResolver = eventTypeResolver;
        _serializerSettingsFactory = serializerSettingsFactory;
        _settings = settings;
    }

    public async ValueTask<EventData> SerializeAsync<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId
        => new(
            Uuid.NewUuid(),
            @event.GetType().Name,
            await SerializeEventData(aggregateId, @event),
            SerializeEventMetadata(aggregateId, correlationId?.Id, causationId?.Id));

    private async ValueTask<ReadOnlyMemory<byte>> SerializeEventData<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event)
        where TAggregateId : IAggregateId
        => Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                @event,
                await _serializerSettingsFactory.CreateForSerializationAsync(
                    @event.GetType(),
                    StreamNamingConvention.GetAggregateStreamName(aggregateId),
                    _settings?.SerializerSettings)));

    private static ReadOnlyMemory<byte>? SerializeEventMetadata<TAggregateId>(
        TAggregateId aggregateId,
        Guid? correlationId,
        Guid? causationId)
        where TAggregateId : notnull
        => Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(
                new EventMetadata(JToken.FromObject(aggregateId), correlationId, causationId),
                MetadataSerializerSettings));

    public async ValueTask<ResolvedEvent> DeserializeAsync(global::KurrentDB.Client.ResolvedEvent resolvedEvent)
    {
        var metadata = DeserializeEventMetadata(resolvedEvent.Event);
        var @event = await DeserializeEventDataAsync(resolvedEvent.Event.EventStreamId, resolvedEvent.Event);

        return new ResolvedEvent(
            new EventId(resolvedEvent.Event.EventId.ToGuid()),
            resolvedEvent.Event.EventStreamId,
            metadata?.AggregateId ?? JValue.CreateNull(),
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
            MetadataSerializerSettings);

    private async ValueTask<IDomainEvent?> DeserializeEventDataAsync(string streamName, EventRecord eventRecord)
    {
        if (_eventTypeResolver.GetEventType(eventRecord.EventType) is not { } eventType) return null;

        return JsonConvert.DeserializeObject(
                Encoding.UTF8.GetString(eventRecord.Data.Span),
                eventType,
                await _serializerSettingsFactory.CreateForDeserializationAsync(
                    streamName,
                    _settings?.SerializerSettings))
            as IDomainEvent;
    }
}