namespace EventSourcingDotNet.LiteDb;

internal sealed class LiteDbEventStore<TAggregateId> : IEventStore<TAggregateId> 
    where TAggregateId : IAggregateId
{
    private readonly ILiteDbEventStream _eventStream;
    private readonly IEventSerializer _eventSerializer;

    public LiteDbEventStore(ILiteDbEventStream eventStream, IEventSerializer eventSerializer)
    {
        _eventStream = eventStream;
        _eventSerializer = eventSerializer;
    }

    public IAsyncEnumerable<ResolvedEvent> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion)
        => _eventStream.ReadEventsAsync(TAggregateId.AggregateName, aggregateId.AsString(), fromVersion)
            .Select(_eventSerializer.Deserialize);

    public async ValueTask<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId = null,
        CausationId? causationId = null)
        => await _eventStream.AppendEventsAsync(
            TAggregateId.AggregateName,
            aggregateId.AsString(),
            expectedVersion, 
            events, 
            correlationId, 
            causationId);
}