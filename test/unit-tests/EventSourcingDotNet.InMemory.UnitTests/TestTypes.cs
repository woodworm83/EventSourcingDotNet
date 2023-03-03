namespace EventSourcingDotNet.InMemory.UnitTests;

internal sealed record TestEvent : IDomainEvent;

internal sealed record OtherTestEvent : IDomainEvent;

internal readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }
    
    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}

internal sealed record TestState : IAggregateState<TestState>
{
    public TestState ApplyEvent(IDomainEvent @event) => this;
}