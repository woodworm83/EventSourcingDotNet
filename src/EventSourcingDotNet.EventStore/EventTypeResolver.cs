using System.Diagnostics.CodeAnalysis;

namespace EventSourcingDotNet.EventStore;

// ReSharper disable once UnusedTypeParameter
[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed")]
internal interface IEventTypeResolver<TAggregateId>
{
    Type? GetEventType(string eventName);
}

internal sealed class EventTypeResolver<TAggregateId> : IEventTypeResolver<TAggregateId> 
    where TAggregateId : IAggregateId
{
    private readonly IReadOnlyDictionary<string, Type> _eventTypes = AppDomain.CurrentDomain
        .GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IDomainEvent)))
        .ToDictionary(StreamNamingConvention.GetEventTypeName);

    public Type? GetEventType(string eventName)
        => _eventTypes.GetValueOrDefault(eventName);
}