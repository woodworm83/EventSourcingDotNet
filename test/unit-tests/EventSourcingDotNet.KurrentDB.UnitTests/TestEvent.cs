namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record TestEvent(int Value = 0) : IDomainEvent;