using JetBrains.Annotations;

namespace EventSourcingDotNet;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public interface IResolvedEvent
{
    public EventId Id { get; }
    public string StreamName { get; }
    public IAggregateId AggregateId { get; }
    public AggregateVersion AggregateVersion { get; }
    public StreamPosition StreamPosition { get; }
    public IDomainEvent? Event { get; }
    public DateTime Timestamp { get; }
    public CorrelationId? CorrelationId { get; }
    public CausationId? CausationId { get; }
}