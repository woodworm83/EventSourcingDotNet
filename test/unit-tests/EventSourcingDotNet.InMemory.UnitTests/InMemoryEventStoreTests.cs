using FluentAssertions;
using Moq;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemoryEventStoreTests
{
    [Fact]
    public async Task ShouldReturnAppendedEvents()
    {
        var aggregateId = new TestId();
        var @event = new TestEvent();
        var eventStore = new InMemoryEventStore<TestId>();
        await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);

        var events = await eventStore.ReadEventsAsync(aggregateId, default)
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
        var eventStore = new InMemoryEventStore<TestId>();
        var actualVersion = await eventStore.AppendEventsAsync(aggregateId, new[] {new TestEvent()}, default);

        await eventStore.Awaiting(x => x.AppendEventsAsync(aggregateId, new[] {new TestEvent()}, new AggregateVersion(42)))
            .Should()
            .ThrowAsync<OptimisticConcurrencyException>()
            .Where(exception => exception.ActualVersion == actualVersion);
    }

    [Fact]
    public async Task ShouldIgnoreOtherStreamsForAggregateVersion()
    {
        var eventStore = new InMemoryEventStore<TestId>();
        await eventStore.AppendEventsAsync(new TestId(), new[] {new TestEvent()}, default);

        await eventStore.Awaiting(x => x.AppendEventsAsync(new TestId(), new[] {new TestEvent()}, default))
            .Should()
            .NotThrowAsync();
    }

    [Fact]
    public async Task ShouldYieldAddedEventByAggregateId()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen(aggregateId).Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))));
    }

    [Fact]
    public async Task ShouldYieldAddedEventsByEventType()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen<TestEvent>().Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new[] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))));
    }

    [Fact]
    public async Task ShouldNotYieldAddedEventWhenAggregateIdDoesNotMatch()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen(aggregateId).Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(new TestId(), new [] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }

    [Fact]
    public async Task ShouldNotYieldAddedEventWhenEventTypeIdDoesNotMatch()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>();
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.Listen<OtherTestEvent>().Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new [] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }
}