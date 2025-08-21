namespace EventSourcingDotNet;

public interface IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    IAsyncEnumerable<ResolvedEvent> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion);

    ValueTask<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId, 
        IEnumerable<IDomainEvent> events, 
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null);
}