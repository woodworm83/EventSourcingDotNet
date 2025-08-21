using System.Globalization;

namespace EventSourcingDotNet.UnitTests;

internal readonly record struct TestId(int Id = 0) : IAggregateId
{
    public static string AggregateName => "test";

    public string AsString() => Id.ToString(CultureInfo.InvariantCulture);
}