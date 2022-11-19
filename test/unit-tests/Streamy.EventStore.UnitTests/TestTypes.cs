namespace Streamy.EventStore.UnitTests;

public readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }

    public static string AggregateName => "test";
    public string AsString() => Id.ToString();
}

public sealed record TestState(uint Counter);

public sealed record TestEvent(int Value = default) : IDomainEvent<TestId>;