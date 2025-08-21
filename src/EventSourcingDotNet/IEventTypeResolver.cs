namespace EventSourcingDotNet;

public interface IEventTypeResolver
{
    Type? GetEventType(string eventName);
}