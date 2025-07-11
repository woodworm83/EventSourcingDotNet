using Newtonsoft.Json;

namespace EventSourcingDotNet.Serialization.Json;

public interface IJsonSerializerSettingsFactory
{
    ValueTask<JsonSerializerSettings> CreateForSerializationAsync(
        Type objectType, 
        string? encryptionKeyName = null,
        JsonSerializerSettings? serializerSettings = null);

    ValueTask<JsonSerializerSettings> CreateForDeserializationAsync(
        string? encryptionKeyName = null,
        JsonSerializerSettings? serializerSettings = null);
}