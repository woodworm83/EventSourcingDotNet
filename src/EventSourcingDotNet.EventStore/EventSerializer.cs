using System.Text;
using EventStore.Client;
using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore;

internal interface IEventSerializer<TAggregateId> 
    where TAggregateId : IAggregateId
{
    EventData Serialize(TAggregateId aggregateId, IDomainEvent<TAggregateId> @event);

    IResolvedEvent<TAggregateId>? Deserialize(ResolvedEvent resolvedEvent);
}

internal sealed record EventMetadata<TAggregateId>(TAggregateId AggregateId);

internal sealed class EventSerializer<TAggregateId> : IEventSerializer<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly IEventTypeResolver<TAggregateId> _eventTypeResolver;

    public EventSerializer(IEventTypeResolver<TAggregateId> eventTypeResolver)
    {
        _eventTypeResolver = eventTypeResolver;
    }

    public EventData Serialize(TAggregateId aggregateId, IDomainEvent<TAggregateId> @event)
        => new(Uuid.NewUuid(), @event.GetType().Name, SerializeEventData(@event), SerializeEventMetadata(aggregateId));

    private static ReadOnlyMemory<byte> SerializeEventData(IDomainEvent<TAggregateId> @event)
        => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

    private static ReadOnlyMemory<byte> SerializeEventMetadata(TAggregateId aggregateId)
        => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata<TAggregateId>(aggregateId)));

    public IResolvedEvent<TAggregateId>? Deserialize(ResolvedEvent resolvedEvent)
    {
        if (DeserializeEventMetadata(resolvedEvent.Event) is not { } metadata) return null;
        if (DeserializeEventData(resolvedEvent.Event) is not { } @event) return null;

        return new ResolvedEvent<TAggregateId>(
            metadata.AggregateId,
            new AggregateVersion(resolvedEvent.Event.EventNumber.ToUInt64() + 1), 
            new StreamPosition(resolvedEvent.OriginalEvent.EventNumber.ToUInt64()), 
            @event, 
            resolvedEvent.Event.Created);
    }

    private static EventMetadata<TAggregateId>? DeserializeEventMetadata(EventRecord eventRecord)
        => JsonConvert.DeserializeObject<EventMetadata<TAggregateId>>(
            Encoding.UTF8.GetString(eventRecord.Metadata.Span));

    private IDomainEvent<TAggregateId>? DeserializeEventData(EventRecord eventRecord)
    {
        if (_eventTypeResolver.GetEventType(eventRecord.EventType) is not { } eventType) return null;

        return JsonConvert.DeserializeObject(
            Encoding.UTF8.GetString(eventRecord.Data.Span),
            eventType) as IDomainEvent<TAggregateId>;
    }
}