namespace EventSourcingDotNet;

public interface IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    public IAsyncEnumerable<IResolvedEvent> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion);

    public ValueTask<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId, 
        IEnumerable<IDomainEvent> events, 
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null);
}