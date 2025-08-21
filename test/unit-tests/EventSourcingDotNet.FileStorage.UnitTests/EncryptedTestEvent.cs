namespace EventSourcingDotNet.FileStorage.UnitTests;

public sealed record EncryptedTestEvent(
    [property: Encrypted] string Value)
    : IDomainEvent;