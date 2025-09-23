using JetBrains.Annotations;

namespace EventSourcingDotNet;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record ResolvedEvent<TAggregateId>(
    EventId Id,
    string StreamName,
    TAggregateId AggregateId,
    AggregateVersion AggregateVersion,
    StreamPosition StreamPosition,
    IDomainEvent<TAggregateId>? Event,
    DateTime Timestamp,
    CorrelationId? CorrelationId,
    CausationId? CausationId)
    : IResolvedEvent
    where TAggregateId : IAggregateId
{
    IAggregateId IResolvedEvent.AggregateId => AggregateId;
    IDomainEvent? IResolvedEvent.Event => Event;
}