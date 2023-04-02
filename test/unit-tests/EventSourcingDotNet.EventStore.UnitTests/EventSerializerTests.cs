using System.Text;
using EventSourcingDotNet.Serialization.Json;
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
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(@event: @event);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .Event.Should().Be(@event);
    }

    [Fact]
    public async Task ShouldSetEventNullForUnknownEvents()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(@event: new UnknownEvent());
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Event.Should().BeNull();
    }

    [Fact]
    public async Task ShouldSetTimestamp()
    {
        var timestamp = DateTime.UtcNow;
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(created: timestamp);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public async Task ShouldSetAggregateVersion()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .AggregateVersion.Version.Should().Be(6);
    }

    [Fact]
    public async Task ShouldGetAggregateIdFromMetadata()
    {
        var aggregateId = new TestId();
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(aggregateId: aggregateId);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .GetAggregateId<TestId>().Should().Be(aggregateId);
    }

    [Fact]
    public async Task ShouldNotFailWhenAggregateIdIsNotIncludedInMetadata()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(invalidMetadata: true);
        var serializer = new EventSerializer(_eventTypeResolver, _serializerSettingsFactory);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result.Should().BeAssignableTo<ResolvedEvent>()
            .Which
            .GetAggregateId<TestId>().Should().Be(null);
    }

    [Fact]
    public async Task ShouldSetStreamPosition()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(streamPosition: 5);
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

        await serializer.DeserializeAsync(
            EventDataHelper.CreateResolvedEvent("stream", @event: new EncryptedTestEvent("value")));

        serializerSettingsFactoryMock.Verify(x => x.CreateForDeserializationAsync("stream"));
    }

    private static T? Deserialize<T>(ReadOnlyMemory<byte> data)
        => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data.Span));

    private static IEventTypeResolver CreateEventTypeResolver()
    {
        var eventTypeResolverMock = new Mock<IEventTypeResolver>();
        eventTypeResolverMock.Setup(x => x.GetEventType(nameof(TestEvent)))
            .Returns(typeof(TestEvent));
        eventTypeResolverMock.Setup(x => x.GetEventType(nameof(EncryptedTestEvent)))
            .Returns(typeof(EncryptedTestEvent));
        return eventTypeResolverMock.Object;
    }
}