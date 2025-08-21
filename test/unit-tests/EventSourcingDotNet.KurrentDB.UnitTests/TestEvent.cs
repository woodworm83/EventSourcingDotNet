namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestEvent(int Value = default) : IDomainEvent;