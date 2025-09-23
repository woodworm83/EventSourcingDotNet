namespace EventSourcingDotNet.UnitTests;

internal sealed record TestState(int Value) : IAggregateState<TestState, TestId>
{
    public TestState() : this(0)
    {
    }

    public TestState ApplyEvent(IDomainEvent<TestId> @event)
        => @event switch
        {
            TestEvent testEvent => new(testEvent.NewValue),
            ValueUpdatedEvent valueUpdated => new(valueUpdated.NewValue),
            _ => this,
        };
    
    public EventValidationResult ValidateEvent(IDomainEvent<TestId> @event) 
        => @event switch
        {
            ValueUpdatedEvent valueUpdated => valueUpdated.ValidationResult,
            _ => EventValidationResult.Fire,
        };
}