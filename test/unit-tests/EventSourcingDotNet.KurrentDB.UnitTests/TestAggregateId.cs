namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestAggregateId(Guid Id) : IAggregateId
{
    public TestAggregateId()
        : this(Guid.NewGuid())
    {
    }

    public static string AggregateName => nameof(TestAggregate);

    public string AsString() => Id.ToString("N");
}