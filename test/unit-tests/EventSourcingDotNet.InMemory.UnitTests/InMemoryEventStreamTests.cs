using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemoryEventStreamTests
{
    [Fact]
    public async Task ShouldReturnAppendedEvents()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent();
        var eventStream = new InMemoryEventStream();
        await eventStream.AppendEventsAsync(aggregateId, new[] {@event}, default, default, default);

        var events = await eventStream.ReadEventsAsync()
            .Select(x => x.Event)
            .ToListAsync();
            
        events
            .Should()
            .Equal(@event);
    }

    [Fact]
    public async Task ShouldThrowOptimisticConcurrencyException()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStream();
        var actualVersion = await eventStore.AppendEventsAsync(aggregateId, new[] {new TestEvent()}, default, default, default);

        await eventStore.Awaiting(x => x.AppendEventsAsync(aggregateId, new[] {new TestEvent()}, new AggregateVersion(42), default, default))
            .Should()
            .ThrowAsync<OptimisticConcurrencyException>()
            .Where(exception => exception.ActualVersion == actualVersion);
    }

    [Fact]
    public async Task ShouldIgnoreOtherStreamsForAggregateVersion()
    {
        var eventStore = new InMemoryEventStream();
        await eventStore.AppendEventsAsync(new TestId(), new[] {new TestEvent()}, default, default, default);

        await eventStore.Awaiting(x => x.AppendEventsAsync(new TestId(), new[] {new TestEvent()}, default, default, default))
            .Should()
            .NotThrowAsync();
    }
}