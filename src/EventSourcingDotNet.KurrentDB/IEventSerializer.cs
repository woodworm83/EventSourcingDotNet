using KurrentDB.Client;

namespace EventSourcingDotNet.KurrentDB;

public interface IEventSerializer
{
    public ValueTask<EventData> SerializeAsync<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId;

    public ValueTask<IResolvedEvent?> DeserializeAsync(ResolvedEvent resolvedEvent);
}