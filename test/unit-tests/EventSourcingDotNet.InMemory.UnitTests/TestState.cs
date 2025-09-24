namespace EventSourcingDotNet.InMemory.UnitTests;

internal sealed record TestState : IAggregateState<TestState, TestId>
{
    public static TestState Create(TestId aggregateId) => new();

    public TestState ApplyEvent(IDomainEvent @event) => this;
}