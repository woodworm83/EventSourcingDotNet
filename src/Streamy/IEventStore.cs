namespace Streamy;

public interface IEventStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    IAsyncEnumerable<IResolvedEvent<TAggregateId>> ReadEvents(
        TAggregateId aggregateId,
        AggregateVersion fromVersion);
}

public interface IResolvedEvent<out TAggregateId>
{
    TAggregateId AggregateId { get; }
    AggregateVersion AggregateVersion { get; }
    StreamPosition StreamPosition { get; }
    IDomainEvent Event { get; }
    DateTime Timestamp { get; }
}

public readonly record struct StreamPosition(ulong Position);

public readonly record struct AggregateVersion(ulong Version)
{
    public static AggregateVersion operator ++(AggregateVersion version)
        => new(version.Version + 1);
}