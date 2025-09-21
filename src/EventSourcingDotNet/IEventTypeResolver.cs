namespace EventSourcingDotNet;

public interface IEventTypeResolver
{
    public Type? GetEventType(string eventName);
}