namespace Streamy;

public interface IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    IAsyncEnumerable<IResolvedEvent<TAggregateId>> ReadEventsAsync(
        TAggregateId aggregateId,
        AggregateVersion fromVersion);

    Task<AggregateVersion> AppendEventsAsync(
        TAggregateId aggregateId, 
        IEnumerable<IDomainEvent<TAggregateId>> events, 
        AggregateVersion expectedVersion);
}

public readonly record struct StreamPosition(ulong Position);

public readonly record struct AggregateVersion(ulong Version)
{
    public static AggregateVersion operator ++(AggregateVersion version)
        => new(version.Version + 1);
}