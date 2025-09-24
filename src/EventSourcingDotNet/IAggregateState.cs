namespace EventSourcingDotNet;

public interface IAggregateState<out TSelf, in TId>
    where TSelf : IAggregateState<TSelf, TId> 
    where TId : IAggregateId
{
    public static abstract TSelf Create(TId aggregateId);
    
    public TSelf ApplyEvent(IDomainEvent @event);

    public EventValidationResult ValidateEvent(IDomainEvent @event) => EventValidationResult.Fire;
}