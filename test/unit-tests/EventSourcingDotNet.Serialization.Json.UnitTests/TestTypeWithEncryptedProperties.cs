namespace EventSourcingDotNet.Serialization.Json.UnitTests;

internal sealed record TestTypeWithEncryptedProperties(
    // ReSharper disable once NotAccessedPositionalProperty.Global
    [property: Encrypted] string Property);