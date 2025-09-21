namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestAggregateId(ulong Id) : IAggregateId
{
    public static string AggregateName => nameof(TestAggregate);

    public string AsString() => Id.ToString();
}