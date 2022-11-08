using FluentAssertions;
using Moq;
using Xunit;

namespace Streamy.InMemory.UnitTests;

public class InMemoryEventStoreTests
{
    [Fact]
    public async Task ShouldReturnAppendedEvents()
    {
        var @event = new TestEvent();
        var eventStore = new InMemoryEventStore<TestId>();
        await eventStore.AppendEventsAsync(default, new[] {@event}, default);

        var events = await eventStore.ReadEventsAsync(default, default)
            .Select(x => x.Event)
            .ToListAsync();
            
        events
            .Should()
            .Equal(@event);
    }

    [Fact]
    public async Task ShouldThrowOptimisticConcurrencyException()
    {
        var eventStore = new InMemoryEventStore<TestId>();

        await eventStore.Awaiting(x => x.AppendEventsAsync(new TestId(), new[] {new TestEvent()}, new AggregateVersion(42)))
            .Should()
            .ThrowAsync<OptimisticConcurrencyException>();
    }

    [Fact]
    public async Task ShouldYieldAddedEventByAggregateId()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<IResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen(aggregateId).Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<IResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))));
    }

    [Fact]
    public async Task ShouldYieldAddedEventsByEventType()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<IResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen<TestEvent>().Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<IResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))));
    }

    [Fact]
    public async Task ShouldNotYieldAddedEventWhenAggregateIdDoesNotMatch()
    {
        var aggregateId = new TestId(42);
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<IResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen(aggregateId).Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(new TestId(), new [] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<IResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }

    [Fact]
    public async Task ShouldNotYieldAddedEventWhenEventTypeIdDoesNotMatch()
    {
        var aggregateId = new TestId(42);
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<IResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen<OtherTestEvent>().Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new [] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<IResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }
}