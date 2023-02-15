using Moq;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class InMemoryEventStoreTests
{
    
    [Fact]
    public async Task ShouldYieldAddedEventByAggregateId()
    {
        var aggregateId = new TestId();
        var eventStore = new InMemoryEventStore<TestId>(new InMemoryEventStream());
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.ByAggregateId(aggregateId).Subscribe(observerMock.Object))
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
        var eventStore = new InMemoryEventStore<TestId>(new InMemoryEventStream());
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.ByEventType<TestEvent>().Subscribe(observerMock.Object))
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
        var eventStore = new InMemoryEventStore<TestId>(new InMemoryEventStream());
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.ByAggregateId(aggregateId).Subscribe(observerMock.Object))
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
        var eventStore = new InMemoryEventStore<TestId>(new InMemoryEventStream());
        var observerMock = new Mock<IObserver<ResolvedEvent<TestId>>>();
        var @event = new TestEvent();
        
        using (eventStore.ByEventType<OtherTestEvent>().Subscribe(observerMock.Object))
        {
            await eventStore.AppendEventsAsync(aggregateId, new [] {@event}, default);
        }
        
        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent<TestId>>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }
}