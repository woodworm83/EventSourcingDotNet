using System.Text;
using AwesomeAssertions;
using System.Text.Json;
using Xunit;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed class EventSerializerTests
{
    [Fact]
    public async Task ShouldSetEventType()
    {
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);
        var @event = new TestEvent();

        var result = await serializer.SerializeAsync(new(), @event, correlationId: null, causationId: null);

        result.Type.Should().Be(nameof(TestEvent));
    }

    [Fact]
    public async Task ShouldSerializeEventData()
    {
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);
        var @event = new TestEvent();

        var result = await serializer.SerializeAsync(new(), @event);

        Deserialize<TestEvent>(result.Data)
            .Should()
            .Be(@event);
    }

    [Fact]
    public async Task ShouldDeserializeEventData()
    {
        var @event = new TestEvent();
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(@event: @event);
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result
            .Should()
            .BeAssignableTo<IResolvedEvent>()
            .Which
            .Event
            .Should()
            .Be(@event);
    }

    [Fact]
    public async Task ShouldSetEventNullForUnknownEvents()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(@event: new UnknownEvent());
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result?.Event.Should().BeNull();
    }

    [Fact]
    public async Task ShouldSetTimestamp()
    {
        var timestamp = DateTime.UtcNow;
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(created: timestamp);
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result
            .Should()
            .BeAssignableTo<IResolvedEvent>()
            .Which
            .Timestamp
            .Should()
            .Be(timestamp);
    }

    [Fact]
    public async Task ShouldSetAggregateVersion()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result
            ?.Should()
            .BeAssignableTo<IResolvedEvent>()
            .Which
            .AggregateVersion
            .Version
            .Should()
            .Be(6);
    }

    [Fact]
    public async Task ShouldGetAggregateIdFromMetadata()
    {
        var aggregateId = new TestAggregateId();
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(aggregateId: aggregateId);
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result?.AggregateId.Should().Be(aggregateId);
    }

    [Fact]
    public async Task ShouldNotFailWhenAggregateIdIsNotIncludedInMetadata()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(invalidMetadata: true);
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result?.AggregateId.Should().Be(expected: null);
    }

    [Fact]
    public async Task ShouldSetStreamPosition()
    {
        var resolvedEvent = EventDataHelper.CreateResolvedEvent(streamPosition: 5);
        var serializer = new EventSerializer(TestJsonSerializerContext.Default);

        var result = await serializer.DeserializeAsync(resolvedEvent);

        result
            .Should()
            .BeAssignableTo<IResolvedEvent>()
            .Which
            .StreamPosition
            .Position
            .Should()
            .Be(5);
    }

    private static T? Deserialize<T>(ReadOnlyMemory<byte> data)
        => JsonSerializer.Deserialize<T>(data.Span);
}