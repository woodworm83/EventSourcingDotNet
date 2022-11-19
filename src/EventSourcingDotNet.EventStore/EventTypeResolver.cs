namespace EventSourcingDotNet.Providers.EventStore;

// ReSharper disable once UnusedTypeParameter
internal interface IEventTypeResolver<TAggregateId>
{
    Type? GetEventType(string eventName);
}

internal sealed class EventTypeResolver<TAggregateId> : IEventTypeResolver<TAggregateId> 
    where TAggregateId : IAggregateId
{
    private static readonly IReadOnlyDictionary<string, Type> _eventTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDomainEvent<TAggregateId>)))
        .ToDictionary(type => type.Name);

    public Type? GetEventType(string eventName)
        => _eventTypes.GetValueOrDefault(eventName);
}