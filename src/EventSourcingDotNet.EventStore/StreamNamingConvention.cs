namespace EventSourcingDotNet.EventStore;

internal static class StreamNamingConvention
{
    public static string GetAggregateStreamName<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId
        => $"{TAggregateId.AggregateName}-{aggregateId.AsString()}";

    public static string GetByCategoryStreamName<TAggregateId>()
        where TAggregateId : IAggregateId
        => $"$ce-{TAggregateId.AggregateName}";

    public static string GetByEventStreamName<TEvent>()
        where TEvent : IDomainEvent
        => $"$et-{typeof(TEvent).Name}";

    public static string GetEventTypeName(IDomainEvent @event)
        => GetEventTypeName(@event.GetType());
            
    public static string GetEventTypeName(Type eventType)
        => eventType.Name;
}