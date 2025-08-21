namespace EventSourcingDotNet.InMemory.UnitTests;

internal sealed record TestState : IAggregateState<TestState, TestId>
{
    public TestState ApplyEvent(IDomainEvent @event) => this;
}