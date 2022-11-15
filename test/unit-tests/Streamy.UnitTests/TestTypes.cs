using System.Diagnostics.CodeAnalysis;

namespace Streamy.UnitTests;

internal readonly record struct TestId(int Id = default) : IAggregateId
{
    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}

internal sealed record TestState(int Value)
{
    public TestState() : this(0) { }
}

internal sealed record TestEvent : IDomainEvent<TestId, TestState>
{
    public TestState Apply(TestState state) => state;
}

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record ValueUpdatedEvent(int NewValue) : IDomainEvent<TestId, TestState>
{
    public EventValidationResult ValidationResult { get; init; } = EventValidationResult.Fire;
    
    public TestState Apply(TestState state)
        => state with {Value = NewValue};
    
    public EventValidationResult Validate(TestState state) => ValidationResult;
}