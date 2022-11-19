using System.Text;
using EventSourcingDotNet.Providers.EventStore;
using EventStore.Client;
using Newtonsoft.Json;

namespace EventSourcingDotNet.EventStore.UnitTests;

internal static class EventDataHelper
{
    public static EventData CreateEventData<TAggregateId>(
        TAggregateId aggregateId, 
        IDomainEvent<TAggregateId> @event,
        Guid? eventId = null)
        where TAggregateId : IAggregateId
        => new(
            eventId is null ? Uuid.NewUuid() : Uuid.FromGuid(eventId.Value),
            @event.GetType().Name,
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new EventMetadata<TAggregateId>(aggregateId))));
}