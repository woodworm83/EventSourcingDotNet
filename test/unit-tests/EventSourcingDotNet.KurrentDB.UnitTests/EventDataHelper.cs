using System.Globalization;
using System.Text;
using KurrentDB.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

internal static class EventDataHelper
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() },
        NullValueHandling = NullValueHandling.Ignore,
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
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, SerializerSettings)),
            Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new EventMetadata(JToken.FromObject(aggregateId), correlationId?.Id ?? Guid.NewGuid(),
                        causationId?.Id),
                    SerializerSettings)));


    public static global::KurrentDB.Client.ResolvedEvent CreateResolvedEvent(
        string eventStreamId = "",
        Uuid? uuid = null,
        ulong streamPosition = 0,
        IDomainEvent? @event = null,
        DateTime? created = null,
        CausationId? causationId = null,
        CorrelationId? correlationId = null,
        bool invalidMetadata = false,
        TestId? aggregateId = null)
    {
        return CreateResolvedEvent(
            @event is not null
                ? StreamNamingConvention.GetEventTypeName(@event)
                : StreamNamingConvention.GetEventTypeName(typeof(TestEvent)),
            Serialize(@event ?? new TestEvent()),
            invalidMetadata
                ? new ReadOnlyMemory<byte>()
                : Serialize(new EventMetadata(JToken.FromObject(aggregateId ?? new TestId()),
                    correlationId?.Id ?? Guid.NewGuid(), causationId?.Id)),
            eventStreamId,
            uuid,
            streamPosition,
            created);
    }

    private static global::KurrentDB.Client.ResolvedEvent CreateResolvedEvent(
        string eventType,
        ReadOnlyMemory<byte> data,
        ReadOnlyMemory<byte> metadata,
        string eventStreamId,
        Uuid? uuid,
        ulong streamPosition,
        DateTime? created)
        => new(
            new EventRecord(
                eventStreamId,
                uuid ?? Uuid.NewUuid(),
                new global::KurrentDB.Client.StreamPosition(streamPosition),
                new Position(streamPosition, streamPosition),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    { "type", eventType },
                    { "created", ToUnixEpochTime(created ?? DateTime.UtcNow).ToString(CultureInfo.InvariantCulture) },
                    { "content-type", "application/json" },
                },
                data,
                metadata),
            link: null,
            commitPosition: null);

    public static global::KurrentDB.Client.ResolvedEvent CreateResolvedEvent(
        EventData eventData,
        string eventStreamId = "",
        ulong streamPosition = 0,
        DateTime? created = null)
        => CreateResolvedEvent(
            eventData.Type,
            eventData.Data,
            eventData.Metadata,
            eventStreamId,
            eventData.EventId,
            streamPosition,
            created);

    private static long ToUnixEpochTime(DateTime dateTime)
        => dateTime.Ticks - DateTime.UnixEpoch.Ticks;

    private static ReadOnlyMemory<byte> Serialize(object value)
        => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
}