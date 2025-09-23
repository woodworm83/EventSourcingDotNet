using System.Globalization;
using System.Text.Json;
using KurrentDB.Client;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

internal static class EventDataHelper
{
    public static EventData CreateEventData<TAggregateId, TEvent>(
        TAggregateId aggregateId,
        TEvent @event,
        Guid? eventId = null,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId
        where TEvent : IDomainEvent
        => new(
            eventId is null
                ? Uuid.NewUuid()
                : Uuid.FromGuid(eventId.Value),
            StreamNamingConvention.GetEventTypeName(@event),
            JsonSerializer.SerializeToUtf8Bytes(@event),
            JsonSerializer.SerializeToUtf8Bytes(
                new EventMetadata<TAggregateId>(
                    aggregateId,
                    correlationId?.Id ?? Guid.NewGuid(),
                    causationId?.Id)));

    public static ResolvedEvent CreateResolvedEvent(
        string eventStreamId = "",
        Uuid? uuid = null,
        ulong streamPosition = 0,
        IDomainEvent? @event = null,
        DateTime? created = null,
        CausationId? causationId = null,
        CorrelationId? correlationId = null,
        bool invalidMetadata = false,
        TestAggregateId? aggregateId = null)
    {
        return CreateResolvedEvent(
            @event is not null
                ? StreamNamingConvention.GetEventTypeName(@event)
                : StreamNamingConvention.GetEventTypeName(typeof(TestEvent)),
            Serialize(@event ?? new TestEvent()),
            invalidMetadata
                ? new()
                : Serialize(
                    new EventMetadata<TestAggregateId>(
                        aggregateId ?? new TestAggregateId(),
                        correlationId?.Id ?? Guid.NewGuid(),
                        causationId?.Id)),
            eventStreamId,
            uuid,
            streamPosition,
            created);
    }

    private static ResolvedEvent CreateResolvedEvent(
        string eventType,
        ReadOnlyMemory<byte> data,
        ReadOnlyMemory<byte> metadata,
        string eventStreamId,
        Uuid? uuid,
        ulong streamPosition,
        DateTime? created)
        => new(
            new(
                eventStreamId,
                uuid ?? Uuid.NewUuid(),
                new(streamPosition),
                new(streamPosition, streamPosition),
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

    public static ResolvedEvent CreateResolvedEvent(
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

    private static long ToUnixEpochTime(DateTime dateTime) => dateTime.Ticks - DateTime.UnixEpoch.Ticks;

    private static ReadOnlyMemory<byte> Serialize(object value) => JsonSerializer.SerializeToUtf8Bytes(value);
}