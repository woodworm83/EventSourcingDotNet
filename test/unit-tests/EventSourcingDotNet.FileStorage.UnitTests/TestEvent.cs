namespace EventSourcingDotNet.FileStorage.UnitTests;

public sealed record TestEvent(int Value = default) : IDomainEvent;