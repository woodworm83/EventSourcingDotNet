namespace EventSourcingDotNet.EventStore;

internal interface IEventTypeResolver
{
    Type? GetEventType(string eventName);
}

internal sealed class EventTypeResolver : IEventTypeResolver
{
    private readonly IReadOnlyDictionary<string, Type> _eventTypes;

    public EventTypeResolver()
    {
        var eventTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDomainEvent)))
            .ToList();
            
        _eventTypes = eventTypes.ToDictionary(StreamNamingConvention.GetEventTypeName);
    }

    public Type? GetEventType(string eventName)
        => _eventTypes.GetValueOrDefault(eventName);
}