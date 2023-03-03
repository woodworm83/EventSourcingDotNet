namespace EventSourcingDotNet.EventStore.UnitTests;

public readonly record struct TestId(Guid Id) : IAggregateId
{
    public TestId() : this(Guid.NewGuid()) { }

    public static string AggregateName => "test";
    public string AsString() => Id.ToString();
}

public sealed record TestEvent(int Value = default) : IDomainEvent;

public sealed record ByTypeEvent(Guid Id) : IDomainEvent
{
    public ByTypeEvent() : this(Guid.NewGuid())
    {
    }
}

public sealed record EncryptedTestEvent(
    [property: Encrypted] string Value)
    : IDomainEvent;

internal sealed class TestEventTypeResolver : IEventTypeResolver
{
    public Type? GetEventType(string eventName)
        => eventName switch
        {
            nameof(TestEvent) => typeof(TestEvent),
            nameof(ByTypeEvent) => typeof(ByTypeEvent),
            nameof(EncryptedTestEvent) => typeof(EncryptedTestEvent),
            _ => null
        };
}