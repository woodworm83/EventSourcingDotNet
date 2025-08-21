namespace EventSourcingDotNet.KurrentDB.UnitTests;

internal sealed class TestEventTypeResolver : IEventTypeResolver
{
    public Type? GetEventType(string eventName)
        => eventName switch
        {
            nameof(TestEvent) => typeof(TestEvent),
            nameof(ByTypeEvent) => typeof(ByTypeEvent),
            nameof(EncryptedTestEvent) => typeof(EncryptedTestEvent),
            _ => null,
        };
}