using System.Diagnostics.CodeAnalysis;

namespace Streamy.UnitTests;

internal readonly record struct TestId(int Id = default) : IAggregateId
{
    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}

internal sealed record TestState(int Value) : IAggregateState<TestState, TestId>
{
    public static TestState New { get; } = new(0);

    public static string AggregateName => "test";

    public static string GetIdAsString(TestId id) => id.Id.ToString();
}

internal sealed record TestEvent : IDomainEvent<TestState>
{
    public TestState Apply(TestState state) => state;
}

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record ValueUpdatedEvent(int NewValue) : IDomainEvent<TestState>
{
    public EventValidationResult ValidationResult { get; init; } = EventValidationResult.Fire;
    
    public TestState Apply(TestState state)
        => state with {Value = NewValue};
    
    public EventValidationResult Validate(TestState state) => ValidationResult;
}