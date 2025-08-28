using System.Reactive.Concurrency;
using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemorySnapshotTests
{
    [Fact]
    public async Task ShouldReturnNullWhenNoEventsAreStored()
    {
        var eventStore = new InMemoryEventStream(ImmediateScheduler.Instance);
        var snapshot = new InMemorySnapshotStore<TestId, TestState>(new EventListener(eventStore));

        var result = await snapshot.GetAsync(new TestId());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnSnapshot()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStream(ImmediateScheduler.Instance);

        await eventStore.AppendEventsAsync(
            aggregateId,
            [new TestEvent()],
            new AggregateVersion(),
            correlationId: null,
            causationId: null);

        var snapshotStore = new InMemorySnapshotStore<TestId, TestState>(new EventListener(eventStore));
        await snapshotStore.StartAsync(CancellationToken.None);

        var result = await snapshotStore.GetAsync(aggregateId);

        await snapshotStore.StopAsync(CancellationToken.None);

        result.Should().BeOfType<Aggregate<TestId, TestState>>();
        result.Id.Should().Be(aggregateId);
        result.Version.Should().Be(new AggregateVersion(1));
        result.State.Should().Be(new TestState());
        result.UncommittedEvents.Should().BeEmpty();
    }
}