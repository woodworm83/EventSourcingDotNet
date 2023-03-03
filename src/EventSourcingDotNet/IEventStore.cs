using System.Diagnostics.CodeAnalysis;

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

public readonly record struct CorrelationId(Guid Id)
{
    public CorrelationId()
        : this(Guid.NewGuid())
    { }
}

public readonly record struct CausationId(Guid Id)
{
    public CausationId()
        : this(Guid.NewGuid())
    { }
    
    public static implicit operator CausationId(EventId eventId)
        => new(eventId.Id);

    public static implicit operator EventId(CausationId causationId)
        => new(causationId.Id);
    
    [return: NotNullIfNotNull(nameof(id))]
    internal static CausationId? FromGuid(Guid? id)
        => id.HasValue
            ? new CausationId(id.Value)
            : null;
}

public readonly record struct StreamPosition(ulong Position);

public readonly record struct AggregateVersion(ulong Version)
{
    public static AggregateVersion operator ++(AggregateVersion version)
        => new(version.Version + 1);
}