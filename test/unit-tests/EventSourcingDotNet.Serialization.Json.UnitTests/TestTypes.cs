namespace EventSourcingDotNet.Serialization.Json.UnitTests;

internal readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }

    public static string AggregateName => "test";

    public string AsString() => Id.ToString();
}

internal sealed record TestTypeWithoutEncryptedProperties(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string Property);

internal sealed record TestTypeWithEncryptedProperties(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    [property: Encrypted] string Property);