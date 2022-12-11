using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemorySnapshotTests
{
    [Fact]
    public async Task ShouldReturnNullWhenNoEventsArePublished()
    {
        var eventPublisherMock = new Mock<IEventListener<TestId>>();
        eventPublisherMock.Setup(x => x.ByCategory(It.IsAny<StreamPosition>()))
            .Returns(Observable.Never<ResolvedEvent<TestId>>());
        var snapshot = new InMemorySnapshot<TestId, TestState>(eventPublisherMock.Object);

        using (Run(snapshot))
        {
            var result = await snapshot.GetLatestSnapshotAsync(new TestId());
                
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task ShouldReturnSnapshot()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent();
        var eventPublisherMock = new Mock<IEventListener<TestId>>();
        eventPublisherMock.Setup(x => x.ByCategory(It.IsAny<StreamPosition>()))
            .Returns(Observable.Return(new ResolvedEvent<TestId> {AggregateId = aggregateId, Event = @event}));
        var snapshot = new InMemorySnapshot<TestId, TestState>(eventPublisherMock.Object);

        using (Run(snapshot))
        {
            var result = await snapshot.GetLatestSnapshotAsync(aggregateId);
                
            result.Should().BeOfType<Aggregate<TestId, TestState>>()
                .Which
                .Id.Should().Be(aggregateId);
        }
    }

    private IDisposable Run(IHostedService service)
        => Observable.Create<Unit>(
            async (_, stoppingToken) =>
            {
                await service.StartAsync(CancellationToken.None);
                var tcs = new TaskCompletionSource();
                stoppingToken.Register(tcs.SetResult);
                await tcs.Task;
                await service.StopAsync(CancellationToken.None);
            })
            .Subscribe();

}