using KurrentDB.Client;

namespace EventSourcingDotNet.KurrentDB;

internal interface IEventSerializer
{
    ValueTask<EventData> SerializeAsync<TAggregateId>(
        TAggregateId aggregateId,
        IDomainEvent @event,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        where TAggregateId : IAggregateId;

    ValueTask<ResolvedEvent> DeserializeAsync(global::KurrentDB.Client.ResolvedEvent resolvedEvent);
}