using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.EventStore.UnitTests;

public class ResolvedEventTests
{
    [Fact]
    public void ShouldInitializeAggregateVersion()
    {
        var version = new AggregateVersion(42);

        var resolvedEvent = new ResolvedEvent {AggregateVersion = version};

        resolvedEvent.AggregateVersion.Should().Be(version);
    }

    [Fact]
    public void ShouldInitializeStreamPosition()
    {
        var streamPosition = new StreamPosition(42);

        var resolvedEvent = new ResolvedEvent {StreamPosition = streamPosition};

        resolvedEvent.StreamPosition.Should().Be(streamPosition);
    }

    [Fact]
    public void ShouldInitializeEvent()
    {
        var @event = new TestEvent();

        var resolvedEvent = new ResolvedEvent {Event = @event};

        resolvedEvent.Event.Should().Be(@event);
    }

    [Fact]
    public void ShouldInitializeTimestamp()
    {
        var timestamp = DateTime.Today;

        var resolvedEvent = new ResolvedEvent {Timestamp = timestamp};

        resolvedEvent.Timestamp.Should().Be(timestamp);
    }
}