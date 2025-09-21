using System.Text.Json.Serialization;

namespace EventSourcingDotNet;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(AesEncryptedValue))]
[JsonSerializable(typeof(AesEncryptedValue?))]
internal sealed partial class AesCryptoProviderSerializerContext : JsonSerializerContext;