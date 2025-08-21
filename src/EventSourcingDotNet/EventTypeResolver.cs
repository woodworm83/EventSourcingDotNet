namespace EventSourcingDotNet;

internal sealed class EventTypeResolver : IEventTypeResolver
{
    private readonly IReadOnlyDictionary<string, Type> _eventTypes;

    public EventTypeResolver()
    {
        _eventTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDomainEvent)))
            .ToDictionary(type => type.Name, StringComparer.Ordinal);
    }

    public Type? GetEventType(string eventName)
        => _eventTypes.GetValueOrDefault(eventName);
}