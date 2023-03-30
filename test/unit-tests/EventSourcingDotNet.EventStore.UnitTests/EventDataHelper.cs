using System.Text;
using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.EventStore.UnitTests;

internal static class EventDataHelper
{
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver{NamingStrategy = new CamelCaseNamingStrategy()},
        NullValueHandling = NullValueHandling.Ignore
    };
    
    public static EventData CreateEventData<TAggregateId>(
        TAggregateId aggregateId, 
        IDomainEvent @event,
        Guid? eventId = null,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId
        => new(
            eventId is null ? Uuid.NewUuid() : Uuid.FromGuid(eventId.Value),
            StreamNamingConvention.GetEventTypeName(@event),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, _serializerSettings)),
            Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new EventMetadata(JToken.FromObject(aggregateId), correlationId?.Id ?? Guid.NewGuid(), causationId?.Id),
                    _serializerSettings)));
}