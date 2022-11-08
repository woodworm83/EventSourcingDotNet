namespace Streamy.InMemory.UnitTests;

internal sealed record TestEvent : IDomainEvent<TestState>
{
    public TestState Apply(TestState state) => state;
}

internal sealed record OtherTestEvent : IDomainEvent<TestState>
{
    public TestState Apply(TestState state) => state;
}

internal readonly record struct TestId(int Id = default) : IAggregateId
{
    public static string AggregateName => "test";

    public string AsString() => string.Empty;
}

internal sealed record TestState: IAggregateState<TestState, TestId>
{
    public static TestState New { get; } = new TestState();
}