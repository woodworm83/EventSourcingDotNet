namespace EventSourcingDotNet.EventStore;

internal static class StreamNamingConvention
{
    public static string GetAggregateStreamName<TAggregateId>(TAggregateId aggregateId)
        where TAggregateId : IAggregateId
        => $"{TAggregateId.AggregateName}-{aggregateId.AsString()}";

    public static string GetByCategoryStreamName<TAggregateId>()
        where TAggregateId : IAggregateId
        => $"$ce-{TAggregateId.AggregateName}";

    public static string GetByEventStreamName<TAggregateId, TEvent>()
        where TAggregateId : IAggregateId 
        where TEvent : IDomainEvent<TAggregateId>
        => $"$et-{TAggregateId.AggregateName}-{typeof(TEvent).Name}";

    public static string GetEventTypeName<TAggregateId>(IDomainEvent<TAggregateId> @event)
        where TAggregateId : IAggregateId
        => GetEventTypeName<TAggregateId>(@event.GetType());
            
    public static string GetEventTypeName<TAggregateId>(Type eventType)
        where TAggregateId : IAggregateId
        => $"{TAggregateId.AggregateName}-{eventType.Name}";
}