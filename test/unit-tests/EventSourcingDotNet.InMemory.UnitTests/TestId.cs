namespace EventSourcingDotNet.InMemory.UnitTests;

internal readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }
    
    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}