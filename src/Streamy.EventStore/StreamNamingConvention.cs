namespace Streamy.EventStore;

internal interface IStreamNamingConvention<TAggregateId>
    where TAggregateId : IAggregateId
{
    string GetAggregateStreamName(TAggregateId aggregateId);

    string GetByCategoryStreamName();
    
    string GetByEventStreamName<TEvent>()
        where TEvent : IDomainEvent<TAggregateId>;
}

internal sealed class StreamNamingConvention<TAggregateId> : IStreamNamingConvention<TAggregateId>
    where TAggregateId : IAggregateId
{
    public string GetAggregateStreamName(TAggregateId aggregateId)
        => $"{TAggregateId.AggregateName}-{aggregateId.AsString()}";

    public string GetByCategoryStreamName()
        => $"$ce-{TAggregateId.AggregateName}";

    public string GetByEventStreamName<TEvent>() 
        where TEvent : IDomainEvent<TAggregateId>
        => $"$et-{typeof(TEvent).Name}";
}