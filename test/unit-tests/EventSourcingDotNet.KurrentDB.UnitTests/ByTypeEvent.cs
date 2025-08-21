namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record ByTypeEvent(Guid Id) : IDomainEvent
{
    public ByTypeEvent() : this(Guid.NewGuid())
    {
    }
}