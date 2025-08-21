namespace EventSourcingDotNet.InMemory;

internal interface IInMemoryEventStream
{
    public ValueTask<AggregateVersion> AppendEventsAsync<TAggregateId>(TAggregateId aggregateId,
        IEnumerable<IDomainEvent> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId;

    public IAsyncEnumerable<ResolvedEvent> ReadEventsAsync(StreamPosition fromStreamPosition = default);

    public IObservable<ResolvedEvent> Listen(StreamPosition fromStreamPosition = default);
}