using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.UnitTests;

[SuppressMessage("ReSharper", "WithExpressionModifiesAllMembers")]
internal sealed record ValueUpdatedEvent(int NewValue) : IDomainEvent
{
    public EventValidationResult ValidationResult { get; init; } = EventValidationResult.Fire;

    public TestState Apply(TestState state)
        => state with {Value = NewValue};
}