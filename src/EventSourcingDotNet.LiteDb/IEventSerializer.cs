using LiteDB;

namespace EventSourcingDotNet.LiteDb;

internal interface IEventSerializer
{
    EventRecord Serialize<TAggregateId>(
        TAggregateId aggregateId, 
        IDomainEvent @event,
        long aggregateVersion,
        long streamPosition,
        Guid correlationId,
        Guid causationId)
        where TAggregateId : IAggregateId;

    ResolvedEvent Deserialize(EventRecord @event);
}

internal sealed class EventSerializer : IEventSerializer
{
    private readonly IEventTypeResolver _eventTypeResolver;
    private readonly BsonMapper _mapper = new();

    public EventSerializer(IEventTypeResolver eventTypeResolver)
    {
        _eventTypeResolver = eventTypeResolver;
    }

    public EventRecord Serialize<TAggregateId>(
        TAggregateId aggregateId, 
        IDomainEvent @event,
        long aggregateVersion,
        long streamPosition,
        Guid correlationId,
        Guid causationId)
        where TAggregateId : IAggregateId
        => new(
            new ObjectId(Guid.NewGuid().ToString()),
            TAggregateId.AggregateName,
            aggregateId.AsString(),
            streamPosition,
            aggregateVersion,
            @event.GetType().Name,
            correlationId,
            causationId,
            DateTime.Now, 
            _mapper.ToDocument(@event));

    public ResolvedEvent Deserialize(EventRecord @event)
        => new(
            new EventId(Guid.Parse(@event.Id.ToString())),
            $"{@event.AggregateName}-{@event.AggregateId}",
            new AggregateVersion((ulong) @event.AggregateVersion),
            new StreamPosition((ulong) @event.StreamPosition),
            _eventTypeResolver.GetEventType(@event.EventType) is { } eventType
                ? _mapper.ToObject(eventType, @event.EventData) as IDomainEvent
                : null,
            @event.Timestamp,
            new CorrelationId(@event.CorrelationId),
            @event.CausationId.HasValue ? new CausationId(@event.CausationId.Value) : null);
}