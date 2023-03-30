using System.Reactive.Linq;

namespace EventSourcingDotNet.InMemory;

internal sealed class EventListener : IEventListener
{
    private readonly IInMemoryEventStream _eventStream;

    public EventListener(IInMemoryEventStream eventStream)
    {
        _eventStream = eventStream;
    }

    public IObservable<ResolvedEvent> ByAggregate<TAggregateId>(
        TAggregateId aggregateId,
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId, IEquatable<TAggregateId>
        => _eventStream.Listen(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.StreamName.Equals(
                $"{TAggregateId.AggregateName}-{aggregateId.AsString()}"));

    public IObservable<ResolvedEvent> ByCategory<TAggregateId>(
        StreamPosition fromStreamPosition = default)
        where TAggregateId : IAggregateId
        => _eventStream.Listen(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.StreamName.StartsWith($"{TAggregateId.AggregateName}-"));

    public IObservable<ResolvedEvent> ByEventType<TEvent>(
        StreamPosition fromStreamPosition = default)
        where TEvent : IDomainEvent
        => _eventStream.Listen(fromStreamPosition)
            .Where(resolvedEvent => resolvedEvent.Event is TEvent);
}