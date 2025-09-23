namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record ByTypeEvent(Guid Id) : IDomainEvent<EventListenerTests.ByEventTypeId>
{
    public ByTypeEvent() : this(Guid.NewGuid())
    {
    }
}