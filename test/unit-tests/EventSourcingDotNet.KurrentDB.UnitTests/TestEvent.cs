namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestEvent(Guid Id) : IDomainEvent<TestAggregateId>
{
    public TestEvent()
        : this(Guid.NewGuid())
    {
    }
}