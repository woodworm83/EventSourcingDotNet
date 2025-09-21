namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestAggregate : IAggregateState<TestAggregate, TestAggregateId>
{
    public TestAggregate ApplyEvent(IDomainEvent @event) => this;
}