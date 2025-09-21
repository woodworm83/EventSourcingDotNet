using System.Text.Json.Serialization;

namespace EventSourcingDotNet;

internal readonly record struct AesEncryptedValue(
    [property: JsonPropertyName("iv")] byte[] InitializationVector,
    [property: JsonPropertyName("cypher")] byte[] CypherText);