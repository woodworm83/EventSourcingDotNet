using FluentAssertions;
using Moq;
using Xunit;
#pragma warning disable CS1998

namespace Streamy.UnitTests;

public class AggregateRepositoryTests
{
    [Fact]
    public async Task ShouldCreateNewAggregate()
    {
        var eventStore = MockEventStore();
        var repository = new AggregateRepository<TestId, TestState>(eventStore);
        
        var result = await repository.GetById(new TestId(42));

        result.Id.Id.Should().Be(42);
    }

    [Fact]
    public async Task ShouldApplyEventFromEventStore()
    {
        var @event = new ValueUpdatedEvent(42);
        var eventStore = MockEventStore(@event);
        var repository = new AggregateRepository<TestId, TestState>(eventStore);

        var result = await repository.GetById(new TestId());

        result.State.Value.Should().Be(@event.NewValue);
    }

    [Fact]
    public async Task ShouldGetSnapshotFromSnapshotProvider()
    {
        var snapshot = new Aggregate<TestId, TestState>(new TestId());
        var snapshotProviderMock = new Mock<ISnapshotProvider<TestId, TestState>>();
        snapshotProviderMock.Setup(x => x.GetLatestSnapshot(It.IsAny<TestId>()))
            .ReturnsAsync(snapshot);
        var eventStore = MockEventStore();
        var repository = new AggregateRepository<TestId, TestState>(eventStore, snapshotProviderMock.Object);

        var aggregate = await repository.GetById(snapshot.Id);

        aggregate.Should().BeSameAs(snapshot);
    }

    private static IEventStore<TestId> MockEventStore(params IDomainEvent[] events)
    {
        var mock = new Mock<IEventStore<TestId>>();
        mock.Setup(x => x.ReadEvents(It.IsAny<TestId>(), It.IsAny<AggregateVersion>()))
            .Returns<TestId, AggregateVersion>(ResolveEvents);
        return mock.Object;

        async IAsyncEnumerable<IResolvedEvent<TestId>> ResolveEvents(TestId aggregateId, AggregateVersion currentVersion)
        {
            var streamPosition = currentVersion.Version;
            foreach (var @event in events)
            {
                yield return new ResolvedEvent(
                    aggregateId,
                    ++currentVersion,
                    new StreamPosition(streamPosition++),
                    @event,
                    DateTime.UtcNow);
            }
        }
    }

    private readonly record struct ResolvedEvent(
            TestId AggregateId,
            AggregateVersion AggregateVersion,
            StreamPosition StreamPosition,
            IDomainEvent Event,
            DateTime Timestamp)
        : IResolvedEvent<TestId>;
}