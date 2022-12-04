using System.Text;
using EventStore.Client;
using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore.UnitTests;

internal static class EventDataHelper
{
    public static EventData CreateEventData<TAggregateId>(
        TAggregateId aggregateId, 
        IDomainEvent<TAggregateId> @event,
        Guid? eventId = null,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId
        => new(
            eventId is null ? Uuid.NewUuid() : Uuid.FromGuid(eventId.Value),
            StreamNamingConvention.GetEventTypeName(@event),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new EventMetadata<TAggregateId>(aggregateId, correlationId?.Id ?? Guid.NewGuid(), causationId?.Id))));
}