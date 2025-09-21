namespace EventSourcingDotNet;

public interface IAggregateState<out TSelf, TId>
    where TSelf : IAggregateState<TSelf, TId>
{
    public TSelf ApplyEvent(IDomainEvent @event);

    public EventValidationResult ValidateEvent(IDomainEvent @event)
        => EventValidationResult.Fire;
}