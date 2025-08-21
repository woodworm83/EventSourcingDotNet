namespace EventSourcingDotNet.UnitTests;

internal sealed record TestState(int Value) : IAggregateState<TestState, TestId>
{
    public TestState() : this(0)
    {
    }

    public TestState ApplyEvent(IDomainEvent @event)
        => @event switch
        {
            TestEvent testEvent => new TestState(testEvent.NewValue),
            ValueUpdatedEvent valueUpdated => new TestState(valueUpdated.NewValue),
            _ => this,
        };
    
    public EventValidationResult ValidateEvent(IDomainEvent @event) 
        => @event switch
        {
            ValueUpdatedEvent valueUpdated => valueUpdated.ValidationResult,
            _ => EventValidationResult.Fire,
        };
}