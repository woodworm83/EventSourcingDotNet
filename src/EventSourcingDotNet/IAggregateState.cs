namespace EventSourcingDotNet;

public interface IAggregateState<out TSelf, TId>
    where TSelf : IAggregateState<TSelf, TId>
{
    TSelf ApplyEvent(IDomainEvent @event);

    EventValidationResult ValidateEvent(IDomainEvent @event)
        => EventValidationResult.Fire;
}