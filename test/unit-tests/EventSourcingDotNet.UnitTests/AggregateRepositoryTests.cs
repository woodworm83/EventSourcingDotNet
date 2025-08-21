using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

#pragma warning disable CS1998

namespace EventSourcingDotNet.UnitTests;

public class AggregateRepositoryTests
{
    [Fact]
    public async Task ShouldCreateNewAggregate()
    {
        var eventStoreMock = MockEventStore();
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object);

        var result = await repository.GetByIdAsync(new TestId(42));

        result.Id.Id.Should().Be(42);
    }

    [Fact]
    public async Task ShouldApplyEventFromEventStore()
    {
        var @event = new ValueUpdatedEvent(42);
        var eventStoreMock = MockEventStore(@event);
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object);

        var result = await repository.GetByIdAsync(new TestId());

        result.State.Value.Should().Be(@event.NewValue);
    }

    [Fact]
    public async Task ShouldGetSnapshotFromSnapshotProvider()
    {
        var snapshot = new Aggregate<TestId, TestState>(new TestId());
        var snapshotProviderMock = new Mock<ISnapshotStore<TestId, TestState>>();

        snapshotProviderMock
            .Setup(x => x.GetAsync(It.IsAny<TestId>()))
            .ReturnsAsync(snapshot);

        var eventStoreMock = MockEventStore();
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object, snapshotProviderMock.Object);

        var aggregate = await repository.GetByIdAsync(snapshot.Id);

        aggregate.Should().BeSameAs(snapshot);
    }

    [Fact]
    public async Task ShouldAppendUncommittedEventsToEventStream()
    {
        var @event = new TestEvent();
        var eventStoreMock = MockEventStore();
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object);
        var aggregate = new Aggregate<TestId, TestState>(new TestId())
            .AddEvent(@event);

        await repository.SaveAsync(aggregate);

        eventStoreMock.Verify(x => x.AppendEventsAsync(
            aggregate.Id,
            It.Is<IEnumerable<IDomainEvent>>(l => l.SequenceEqual(aggregate.UncommittedEvents)),
            aggregate.Version,
            It.IsAny<CorrelationId?>(),
            It.IsAny<CausationId?>()));
    }

    [Fact]
    public async Task ShouldClearUncommittedEvents()
    {
        var aggregate = new Aggregate<TestId, TestState>(new TestId())
            .AddEvent(new TestEvent());
        var repository = new AggregateRepository<TestId, TestState>(MockEventStore().Object);

        var result = await repository.SaveAsync(aggregate);

        result.UncommittedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldUpdateAggregateVersion()
    {
        var aggregate = new Aggregate<TestId, TestState>(new TestId());
        var eventStoreMock = new Mock<IEventStore<TestId>>();
        eventStoreMock.Setup(x => x.AppendEventsAsync(
                It.IsAny<TestId>(),
                It.IsAny<IEnumerable<IDomainEvent>>(),
                It.IsAny<AggregateVersion>(),
                null,
                null))
            .ReturnsAsync(new AggregateVersion(42));
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object);

        var result = await repository.SaveAsync(aggregate);

        result.Version.Version.Should().Be(42);
    }

    [Fact]
    public async Task ShouldUpdateSnapshotAfterReplay()
    {
        var snapshotProviderMock = new Mock<ISnapshotStore<TestId, TestState>>();
        var eventStoreMock = MockEventStore(new TestEvent());
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object, snapshotProviderMock.Object);

        var aggregate = await repository.GetByIdAsync(new TestId());

        snapshotProviderMock.Verify(x => x.SetAsync(aggregate), Times.Once);
    }

    [Fact]
    public async Task ShouldUpdateSnapshotAfterSave()
    {
        var snapshotProviderMock = new Mock<ISnapshotStore<TestId, TestState>>();
        var eventStoreMock = MockEventStore();
        var repository = new AggregateRepository<TestId, TestState>(eventStoreMock.Object, snapshotProviderMock.Object);

        var aggregate = await repository.SaveAsync(
            new Aggregate<TestId, TestState>(new TestId()).AddEvent(new TestEvent()));

        snapshotProviderMock.Verify(x => x.SetAsync(aggregate), Times.Once);
    }

    private static Mock<IEventStore<TestId>> MockEventStore(params IDomainEvent[] events)
    {
        var mock = new Mock<IEventStore<TestId>>();
        mock.Setup(x => x.ReadEventsAsync(It.IsAny<TestId>(), It.IsAny<AggregateVersion>()))
            .Returns<TestId, AggregateVersion>(ResolveEvents);
        return mock;

        async IAsyncEnumerable<ResolvedEvent> ResolveEvents(TestId aggregateId, AggregateVersion currentVersion)
        {
            var streamPosition = currentVersion.Version;
            foreach (var @event in events)
            {
                yield return new ResolvedEvent(
                    new EventId(Guid.NewGuid()),
                    "",
                    JToken.FromObject(aggregateId),
                    ++currentVersion,
                    new StreamPosition(streamPosition++),
                    @event,
                    DateTime.UtcNow,
                    new CorrelationId(),
                    null);
            }
        }
    }
}