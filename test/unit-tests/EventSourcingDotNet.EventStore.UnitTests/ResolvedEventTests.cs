using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class ResolvedEventTests
{
    [Fact]
    public void ShouldInitializeAggregateId()
    {
        var aggregateId = new TestId();
        
        var resolvedEvent = new ResolvedEvent<TestId> { AggregateId = aggregateId };

        resolvedEvent.AggregateId.Should().Be(aggregateId);
    }

    [Fact]
    public void ShouldInitializeAggregateVersion()
    {
        var version = new AggregateVersion(42);

        var resolvedEvent = new ResolvedEvent<TestId> {AggregateVersion = version};

        resolvedEvent.AggregateVersion.Should().Be(version);
    }

    [Fact]
    public void ShouldInitializeStreamPosition()
    {
        var streamPosition = new StreamPosition(42);

        var resolvedEvent = new ResolvedEvent<TestId> {StreamPosition = streamPosition};

        resolvedEvent.StreamPosition.Should().Be(streamPosition);
    }

    [Fact]
    public void ShouldInitializeEvent()
    {
        var @event = new TestEvent();

        var resolvedEvent = new ResolvedEvent<TestId> {Event = @event};

        resolvedEvent.Event.Should().Be(@event);
    }

    [Fact]
    public void ShouldInitializeTimestamp()
    {
        var timestamp = DateTime.Today;

        var resolvedEvent = new ResolvedEvent<TestId> {Timestamp = timestamp};

        resolvedEvent.Timestamp.Should().Be(timestamp);
    }
}