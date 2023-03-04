using System.Text;
using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class EventSerializerTests
{
    private readonly IEventTypeResolver _eventTypeResolver = CreateEventTypeResolver();

    private readonly JsonSerializerSettingsFactory _serializerSettingsFactory = new(NullLoggerFactory.Instance);

    [Fact]
    public async Task ShouldSetEventType()
    {
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);
        var @event = new TestEvent();

        var result = await serializer.SerializeAsync(new TestId(), @event, null, null);

        result.Type.Should().Be(nameof(TestEvent));
    }

    [Fact]
    public async Task ShouldSerializeEventData()
    {
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);
        var @event = new TestEvent(42);

        var result = await serializer.SerializeAsync(new TestId(), @event, null, null);

        Deserialize<TestEvent>(result.Data)
            .Should()
            .Be(@event);
    } 
    
    [Fact]
    public async Task ShouldDeserializeEventData()
    {
        var @event = new TestEvent(42);
        var resolvedEvent = CreateResolvedEvent(@event: @event);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .Event.Should().Be(@event);
    }

    [Fact]
    public async Task ShouldSetTimestamp()
    {
        var timestamp = DateTime.UtcNow;
        var resolvedEvent = CreateResolvedEvent(created: timestamp);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public async Task ShouldSetAggregateVersion()
    {
        var resolvedEvent = CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .AggregateVersion.Version.Should().Be(6);
    }

    [Fact]
    public async Task ShouldSetStreamPosition()
    {
        var resolvedEvent = CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .StreamPosition.Position.Should().Be(5);
    }

    [Fact]
    public async Task ShouldGetSerializationOptionsFromFactory()
    {
        var aggregateId = new TestId();
        var serializerSettingsFactoryMock = new Mock<IJsonSerializerSettingsFactory>();
        var serializer = new EventSerializer(_eventTypeResolver, serializerSettingsFactoryMock.Object);

        await serializer.SerializeAsync(aggregateId, new EncryptedTestEvent("value"), null, null);
        
        serializerSettingsFactoryMock.Verify(x => x.CreateForSerializationAsync( 
            typeof(EncryptedTestEvent),
            StreamNamingConvention.GetAggregateStreamName(aggregateId)));
    }

    [Fact]
    public async Task ShouldGetDeserializationOptionsFromFactory()
    {
        var serializerSettingsFactoryMock = new Mock<IJsonSerializerSettingsFactory>();
        var serializer = new EventSerializer(_eventTypeResolver, serializerSettingsFactoryMock.Object);

        await serializer.DeserializeAsync(CreateResolvedEvent("stream", @event: new EncryptedTestEvent("value")));
        
        serializerSettingsFactoryMock.Verify(x => x.CreateForDeserializationAsync("stream"));
    }

    private static T? Deserialize<T>(ReadOnlyMemory<byte> data)
        => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data.Span));

    private static ReadOnlyMemory<byte> Serialize(object value)
        => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));

    private static global::EventStore.Client.ResolvedEvent CreateResolvedEvent(
        string eventStreamId = "",
        Uuid? uuid = null,
        ulong streamPosition = 0,
        IDomainEvent? @event = null,
        DateTime? created = null,
        CausationId? causationId = null,
        CorrelationId? correlationId = null,
        bool invalidMetadata = false)
        => new(
            new EventRecord(
                eventStreamId,
                uuid ?? Uuid.NewUuid(),
                new global::EventStore.Client.StreamPosition(streamPosition),
                new Position(streamPosition, streamPosition),
                new Dictionary<string, string>
                {
                    {"type", @event?.GetType().Name ?? nameof(TestEvent)},
                    {"created", ToUnixEpochTime(created ?? DateTime.UtcNow).ToString()},
                    {"content-type", "application/json"}
                },
                Serialize(@event ?? new TestEvent()),
                invalidMetadata
                    ? new ReadOnlyMemory<byte>()
                    : Serialize(new EventMetadata(
                        correlationId?.Id ?? Guid.NewGuid(),
                        causationId?.Id))),
            null,
            null);

    private static IEventTypeResolver CreateEventTypeResolver()
    {
        var eventTypeResolverMock = new Mock<IEventTypeResolver>();
        eventTypeResolverMock.Setup(x => x.GetEventType(nameof(TestEvent)))
            .Returns(typeof(TestEvent));
        eventTypeResolverMock.Setup(x => x.GetEventType(nameof(EncryptedTestEvent)))
            .Returns(typeof(EncryptedTestEvent));
        return eventTypeResolverMock.Object;
    }

    private static long ToUnixEpochTime(DateTime dateTime)
        => dateTime.Ticks - DateTime.UnixEpoch.Ticks;
}