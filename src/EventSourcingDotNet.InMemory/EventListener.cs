using System.Reactive.Linq;

namespace EventSourcingDotNet.InMemory;

internal sealed class EventListener : IEventListener
{
    private readonly IInMemoryEventStream _eventStream;

    public EventListener(IInMemoryEventStream eventStream)
    {
        _eventStream = eventStream;
    }

    public IObservable<ResolvedEvent<TAggregateId>> ByAggregateId<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => ByCategory<TAggregateId>(fromStreamPosition)
            .Where(resolvedEvent => aggregateId.Equals(resolvedEvent.AggregateId));

    public IObservable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => _eventStream.Listen(fromStreamPosition)
            .OfType<ResolvedEvent<TAggregateId>>();

    public IObservable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => _eventStream.Listen(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.Event is TEvent);
}