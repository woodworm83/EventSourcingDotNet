using System.Reactive.Subjects;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace EventSourcingDotNet.InMemory.UnitTests;

public class EventListenerTests
{
    [Fact]
    public void ShouldYieldAddedEventByAggregateId()
    {
        var aggregateId = new TestId();
        var eventStream = MockEventStream(out var publishEvent);
        var eventListener = new EventListener(eventStream);
        var observerMock = new Mock<IObserver<ResolvedEvent>>();
        var @event = new TestEvent();

        using (eventListener.ByAggregate(aggregateId).Subscribe(observerMock.Object))
        {
            publishEvent(aggregateId, @event);
        }

        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent>(e => ReferenceEquals(e.Event, @event))));
    }

    [Fact]
    public void ShouldYieldAddedEventsByEventType()
    {
        var aggregateId = new TestId();
        var eventStream = MockEventStream(out var publishEvent);
        var eventListener = new EventListener(eventStream);
        var observerMock = new Mock<IObserver<ResolvedEvent>>();
        var @event = new TestEvent();

        using (eventListener.ByEventType<TestEvent>().Subscribe(observerMock.Object))
        {
            publishEvent(aggregateId, @event);
        }

        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent>(e => ReferenceEquals(e.Event, @event))));
    }

    [Fact]
    public void ShouldNotYieldAddedEventWhenAggregateIdDoesNotMatch()
    {
        var eventStream = MockEventStream(out var publishEvent);
        var eventListener = new EventListener(eventStream);
        var observerMock = new Mock<IObserver<ResolvedEvent>>();
        var aggregateId = new TestId();
        var @event = new TestEvent();

        using (eventListener.ByAggregate(aggregateId).Subscribe(observerMock.Object))
        {
            publishEvent(new TestId(), @event);
        }

        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }

    [Fact]
    public void ShouldNotYieldAddedEventWhenEventTypeIdDoesNotMatch()
    {
        var aggregateId = new TestId();
        var eventStream = MockEventStream(out var publishEvent);
        var eventListener = new EventListener(eventStream);
        var observerMock = new Mock<IObserver<ResolvedEvent>>();
        var @event = new TestEvent();

        using (eventListener.ByEventType<OtherTestEvent>().Subscribe(observerMock.Object))
        {
            publishEvent(aggregateId, @event);
        }

        observerMock.Verify(
            x => x.OnNext(
                It.Is<ResolvedEvent>(e => ReferenceEquals(e.Event, @event))),
            Times.Never);
    }


    private static IInMemoryEventStream MockEventStream(out Action<TestId, IDomainEvent> publishEvent)
    {
        var eventSubject = new Subject<ResolvedEvent>();
        var eventStreamMock = new Mock<IInMemoryEventStream>();
        eventStreamMock.Setup(x => x.Listen(It.IsAny<StreamPosition>()))
            .Returns(eventSubject);

        publishEvent = (aggregateId, @event) => eventSubject.OnNext(
            new ResolvedEvent(
                new EventId(),
                $"{TestId.AggregateName}-{aggregateId.AsString()}",
                JToken.FromObject(aggregateId),
                default,
                default,
                @event,
                DateTime.Now,
                null,
                null));
        
        return eventStreamMock.Object;
    }
}