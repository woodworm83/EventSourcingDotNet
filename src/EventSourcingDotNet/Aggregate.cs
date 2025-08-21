using System.Collections.Immutable;
using System.Diagnostics.Contracts;

namespace EventSourcingDotNet;

[Pure]
public sealed record Aggregate<TId, TState>(TId Id)
    where TId : IAggregateId
    where TState : IAggregateState<TState, TId>, new()
{
    /// <summary>
    /// Current version of the aggregate
    /// This property is set by aggregate repository when events are replayed
    /// </summary>
    public AggregateVersion Version { get; internal init; }

    /// <summary>
    /// Collection of uncommitted events.
    /// Use <see cref="IAggregateRepository&lt;TId, TState&gt;"/>.Save to store the events in the event stream
    /// </summary>
    public ImmutableList<IDomainEvent> UncommittedEvents { get; internal init; }
        = ImmutableList<IDomainEvent>.Empty;

    /// <summary>
    /// Adds an event to the collection of uncommitted events
    /// Use <see cref="IAggregateRepository&lt;TId, TState&gt;"/>.Save to store the events in the event stream
    /// </summary>
    /// <param name="event">The event to apply and to be added to the collection of uncommitted events</param>
    /// <returns></returns>
    [Pure]
    public Aggregate<TId, TState> AddEvent([Pure]IDomainEvent @event)
        => State.ValidateEvent(@event) switch
        {
            EventValidationResult.Fired
                => this with
                {
                    State = State.ApplyEvent(@event),
                    UncommittedEvents = UncommittedEvents.Add(@event),
                },
            EventValidationResult.Skipped => this,
            EventValidationResult.Failed failed => throw new EventValidationException(failed.Message),
            _ => throw new NotSupportedException($"Validation result is not supported"),
        };

    [Pure]
    public Aggregate<TId, TState> ApplyEvent(ResolvedEvent resolvedEvent)
        => resolvedEvent.Event switch
        {
            null => this,
            var @event => this with
            {
                State = State.ApplyEvent(@event),
                Version = resolvedEvent.AggregateVersion
            },
        };

    /// <summary>
    /// The current state of the aggregate.
    /// You can update the state by adding new events.
    /// </summary>
    public TState State { get; private init; } = new();
}