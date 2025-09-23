namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestAggregateId(ulong Id = 0) : IAggregateId
{
    public static string AggregateName => nameof(TestAggregate);

    public string AsString() => Id.ToString();
}