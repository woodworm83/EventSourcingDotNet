using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.UnitTests;

internal readonly record struct TestId(int Id = default) : IAggregateId
{
    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}

internal sealed record TestState(int Value) : IAggregateState<TestState, TestId>
{
    public TestState() : this(0)
    {
    }

    public TestState ApplyEvent(IDomainEvent @event)
        => @event switch
        {
            TestEvent testEvent => this with {Value = testEvent.NewValue},
            ValueUpdatedEvent valueUpdated => this with {Value = valueUpdated.NewValue},
            _ => this
        };
    
    public EventValidationResult ValidateEvent(IDomainEvent @event) 
        => @event switch
        {
            ValueUpdatedEvent valueUpdated => valueUpdated.ValidationResult,
            _ => EventValidationResult.Fire
        };
}

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record TestEvent(int NewValue = default) : IDomainEvent;

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record ValueUpdatedEvent(int NewValue) : IDomainEvent
{
    public EventValidationResult ValidationResult { get; init; } = EventValidationResult.Fire;

    public TestState Apply(TestState state)
        => state with {Value = NewValue};
}