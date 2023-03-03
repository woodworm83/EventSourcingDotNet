using Newtonsoft.Json;

namespace EventSourcingDotNet.Serialization.Json;

public interface IJsonSerializerSettingsFactory
{
    ValueTask<JsonSerializerSettings> CreateForSerializationAsync(Type objectType, string? encryptionKeyName = null);

    ValueTask<JsonSerializerSettings> CreateForDeserializationAsync(string? encryptionKeyName = null);
}