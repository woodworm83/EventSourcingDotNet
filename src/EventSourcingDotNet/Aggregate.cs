using System.Collections.Immutable;

namespace EventSourcingDotNet;

public interface IAggregateId
{
    static abstract string AggregateName { get; }

    string AsString();
}

public sealed record Aggregate<TId, TState>(
    TId Id) 
    where TId : IAggregateId
    where TState : new()
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
    public ImmutableList<IDomainEvent<TId, TState>> UncommittedEvents { get; internal init; } 
        = ImmutableList<IDomainEvent<TId, TState>>.Empty;

    /// <summary>
    /// Adds an event to the collection of uncommitted events
    /// Use <see cref="IAggregateRepository&lt;TId, TState&gt;"/>.Save to store the events in the event stream
    /// </summary>
    /// <param name="event">The event to apply and to be added to the collection of uncommitted events</param>
    /// <returns></returns>
    public Aggregate<TId, TState> AddEvent(IDomainEvent<TId, TState> @event)
        => @event.Validate(State) switch
        {
            EventValidationResult.Fired 
                => this with
                {
                    State = @event.Apply(State),
                    UncommittedEvents = UncommittedEvents.Add(@event)
                },
            EventValidationResult.Skipped => this,
            EventValidationResult.Failed failed => throw failed.Exception,
            _ => throw new NotSupportedException($"Validation result is not supported")
        };

    /// <summary>
    /// The current state of the aggregate.
    /// You can update the state by adding new events.
    /// </summary>
    public TState State { get; internal init; } = new TState();
}