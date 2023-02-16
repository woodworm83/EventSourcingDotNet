namespace EventSourcingDotNet.FileStorage.UnitTests;

public readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }

    public static string AggregateName => "test";
    public string AsString() => Id.ToString();
}

public sealed record TestEvent(int Value = default) : IDomainEvent;

public sealed record EncryptedTestEvent(
    [property: Encrypted] string Value)
    : IDomainEvent;