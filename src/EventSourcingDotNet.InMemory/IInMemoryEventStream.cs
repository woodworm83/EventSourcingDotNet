namespace EventSourcingDotNet.InMemory;

internal interface IInMemoryEventStream
{
    public ValueTask<AggregateVersion> AppendEventsAsync<TAggregateId>(
        TAggregateId aggregateId,
        IEnumerable<IDomainEvent<TAggregateId>> events,
        AggregateVersion expectedVersion,
        CorrelationId? correlationId,
        CausationId? causationId)
        where TAggregateId : IAggregateId;

    public IAsyncEnumerable<IResolvedEvent> ReadEventsAsync(StreamPosition fromStreamPosition = default);

    public IObservable<IResolvedEvent> Listen(StreamPosition fromStreamPosition = default);
}