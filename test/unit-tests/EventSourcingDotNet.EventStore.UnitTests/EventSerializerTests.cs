using System.Text;
using EventStore.Client;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventSerializerTests
{
    private readonly IEventTypeResolver<TestId> _eventTypeResolver = CreateEventTypeResolver();

    [Fact]
    public void ShouldSetEventType()
    {
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);
        var @event = new TestEvent();

        var result = serializer.Serialize(new TestId(), @event);

        result.Type.Should().Be(nameof(TestEvent));
    }

    [Fact]
    public void ShouldSerializeEventData()
    {
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);
        var @event = new TestEvent(42);

        var result = serializer.Serialize(new TestId(), @event);

        Deserialize<TestEvent>(result.Data)
            .Should()
            .Be(@event);
    }

    [Fact]
    public void ShouldSerializeAggregateIdInEventMetadata()
    {
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);
        var @event = new TestEvent();
        var aggregateId = new TestId();
        
        var result = serializer.Serialize(aggregateId, @event);

        (Deserialize<EventMetadata<TestId>>(result.Metadata)?.AggregateId)
            .Should()
            .Be(aggregateId);
    }

    [Fact]
    public void ShouldDeserializeAggregateIdFromMetadata()
    {
        var aggregateId = new TestId();
        var resolvedEvent = CreateResolvedEvent(aggregateId: aggregateId);
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);
        
        var result = serializer.Deserialize(resolvedEvent)?.AggregateId;
            
        result.Should().Be(aggregateId);
    }

    [Fact]
    public void ShouldDeserializeEventData()
    {
        var @event = new TestEvent(42);
        var resolvedEvent = CreateResolvedEvent(@event: @event);
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);

        var result = serializer.Deserialize(resolvedEvent)?.Event;
            
        result.Should().Be(@event);
    }

    [Fact]
    public void ShouldSetTimestamp()
    {
        var timestamp = DateTime.UtcNow;
        var resolvedEvent = CreateResolvedEvent(created: timestamp);
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);

        var result = serializer.Deserialize(resolvedEvent);

        result?.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void ShouldSetAggregateVersion()
    {
        var resolvedEvent = CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);

        var result = serializer.Deserialize(resolvedEvent);

        result?.AggregateVersion.Version.Should().Be(6);
    }

    [Fact]
    public void ShouldSetStreamPosition()
    {
        var resolvedEvent = CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer<TestId>(_eventTypeResolver);

        var result = serializer.Deserialize(resolvedEvent);

        result?.StreamPosition.Position.Should().Be(5);
    }

    private static T? Deserialize<T>(ReadOnlyMemory<byte> data)
        => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data.Span));

    private static ReadOnlyMemory<byte> Serialize(object value)
        => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));

    private ResolvedEvent CreateResolvedEvent(string eventStreamId = "", Uuid? uuid = null, ulong streamPosition = 0,
        TestEvent? @event = null, TestId? aggregateId = null, DateTime? created = null)
        => new(
            new EventRecord(
                eventStreamId,
                uuid ?? Uuid.NewUuid(),
                new global::EventStore.Client.StreamPosition(streamPosition),
                new Position(streamPosition, streamPosition),
                new Dictionary<string, string>
                {
                    {"type", nameof(TestEvent)},
                    {"created", ToUnixEpochTime(created ?? DateTime.UtcNow).ToString()},
                    {"content-type", "application/json"}
                },
                Serialize(@event ?? new TestEvent()),
                Serialize(new EventMetadata<TestId>(aggregateId ?? new TestId()))),
            null,
            null);

    private static IEventTypeResolver<TestId> CreateEventTypeResolver()
    {
        var eventTypeResolverMock = new Mock<IEventTypeResolver<TestId>>();
        eventTypeResolverMock.Setup(x => x.GetEventType(nameof(TestEvent)))
            .Returns(typeof(TestEvent));
        return eventTypeResolverMock.Object;
    }
    
    private static long ToUnixEpochTime(DateTime dateTime)
        => dateTime.Ticks - DateTime.UnixEpoch.Ticks;
}