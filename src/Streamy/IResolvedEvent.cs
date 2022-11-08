namespace Streamy;

public interface IResolvedEvent<out TAggregateId>
{
    TAggregateId AggregateId { get; }
    AggregateVersion AggregateVersion { get; }
    StreamPosition StreamPosition { get; }
    IDomainEvent Event { get; }
    DateTime Timestamp { get; }
}