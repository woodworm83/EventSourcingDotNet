namespace EventSourcingDotNet;

public interface IAggregateState<out TSelf, TId>
    where TSelf : IAggregateState<TSelf, TId> 
    where TId : IAggregateId
{
    public TSelf ApplyEvent(IDomainEvent<TId> @event);

    public EventValidationResult ValidateEvent(IDomainEvent<TId> @event) => EventValidationResult.Fire;
}