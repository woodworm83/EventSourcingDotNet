using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.UnitTests;

internal readonly record struct TestId(int Id = default) : IAggregateId
{
    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}

internal sealed record TestState(int Value) : IAggregateState<TestId>
{
    public TestState() : this(0)
    {
    }
}

internal sealed record TestEvent(int NewValue = default) : IDomainEvent<TestId, TestState>
{
    public TestState Apply(TestState state) => state with {Value = NewValue};
}

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record ValueUpdatedEvent(int NewValue) : IDomainEvent<TestId, TestState>
{
    public EventValidationResult ValidationResult { get; init; } = EventValidationResult.Fire;

    public TestState Apply(TestState state)
        => state with {Value = NewValue};

    public EventValidationResult Validate(TestState state) => ValidationResult;
}