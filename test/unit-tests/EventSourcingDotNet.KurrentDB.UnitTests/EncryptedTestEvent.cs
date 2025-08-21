namespace EventSourcingDotNet.KurrentDB.UnitTests;

public sealed record EncryptedTestEvent(
    [property: Encrypted] string Value)
    : IDomainEvent;