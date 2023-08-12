using Moq;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class AggregateRepositoryInterfaceTests
{
    [Fact]
    public async Task ShouldGetAggregateFromRepository()
    {
        var aggregateId = new TestId();
        var aggregateRepositoryMock = new Mock<IAggregateRepository<TestId, TestState>>();
        aggregateRepositoryMock.Setup(x => x.GetByIdAsync(aggregateId))
            .ReturnsAsync(new Aggregate<TestId, TestState>(aggregateId));

        await aggregateRepositoryMock.Object.UpdateAsync(new TestId());

        aggregateRepositoryMock.Verify(x => x.GetByIdAsync(aggregateId), Times.Once);
    }

    [Fact]
    public async Task ShouldSaveAggregateToRepository()
    {
        var aggregateId = new TestId();
        var aggregate = new Aggregate<TestId, TestState>(aggregateId);
        var aggregateRepositoryMock = new Mock<IAggregateRepository<TestId, TestState>>();
        aggregateRepositoryMock.Setup(x => x.GetByIdAsync(aggregateId))
            .ReturnsAsync(aggregate);

        await aggregateRepositoryMock.Object.UpdateAsync(new TestId());

        aggregateRepositoryMock.Verify(
            x => x.SaveAsync(
                aggregate, 
                It.IsAny<CorrelationId?>(), 
                It.IsAny<CausationId?>()), Times.Once);
    }

    [Fact]
    public async Task ShouldApplyEvents()
    {
        var aggregateId = new TestId();
        var aggregate = new Aggregate<TestId, TestState>(aggregateId);
        var @event = new TestEvent(42);

        var aggregateRepositoryMock = new Mock<IAggregateRepository<TestId, TestState>>();
        aggregateRepositoryMock.Setup(x => x.GetByIdAsync(aggregateId))
            .ReturnsAsync(aggregate);

        await aggregateRepositoryMock.Object.UpdateAsync(aggregateId, @event);

        aggregateRepositoryMock.Verify(
            x => x.SaveAsync(
                It.Is<Aggregate<TestId, TestState>>(a => a.State.Value == 42),
                It.IsAny<CorrelationId>(),
                It.IsAny<CausationId>()));
    }
}