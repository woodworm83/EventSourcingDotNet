using Newtonsoft.Json;

namespace EventSourcingDotNet.Serialization.Json;

public interface IJsonSerializerSettingsFactory<in TAggregateId>
{
    ValueTask<JsonSerializerSettings> CreateForSerializationAsync(TAggregateId aggregateId, Type objectType);

    ValueTask<JsonSerializerSettings> CreateForDeserializationAsync(TAggregateId aggregateId);
}