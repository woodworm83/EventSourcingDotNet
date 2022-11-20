namespace EventSourcingDotNet.InMemory.UnitTests;

internal sealed record TestEvent : IDomainEvent<TestId, TestState>
{
    public TestState Apply(TestState state) => state;
}

internal sealed record OtherTestEvent : IDomainEvent<TestId, TestState>
{
    public TestState Apply(TestState state) => state;
}

internal readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }
    
    public static string AggregateName => "test";

    public string AsString() => string.Empty;
}

internal sealed record TestState : IAggregateState<TestId>;