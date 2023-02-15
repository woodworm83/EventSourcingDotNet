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
            .Where(resolvedEvent => resolvedEvent.AggregateId.Equals(aggregateId));

    public IObservable<ResolvedEvent<TAggregateId>> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => _eventStream.Listen()
            .SkipWhile(resolvedEvent => resolvedEvent.StreamPosition.Position < fromStreamPosition.Position)
            .OfType<ResolvedEvent<TAggregateId>>();

    public IObservable<ResolvedEvent<TAggregateId>> ByEventType<TAggregateId, TEvent>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        where TEvent : IDomainEvent<TAggregateId>
        => ByCategory<TAggregateId>(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.Event is TEvent);
}