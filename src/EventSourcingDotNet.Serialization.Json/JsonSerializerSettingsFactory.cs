using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

public sealed class JsonSerializerSettingsFactory<TAggregateId> : IJsonSerializerSettingsFactory<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEncryptionKeyStore<TAggregateId>? _encryptionKeyStore;
    private readonly ICryptoProvider? _cryptoProvider;

    public JsonSerializerSettingsFactory(
        ILoggerFactory loggerFactory,
        IEncryptionKeyStore<TAggregateId>? encryptionKeyStore = null,
        ICryptoProvider? cryptoProvider = null)
    {
        _loggerFactory = loggerFactory;
        _encryptionKeyStore = encryptionKeyStore;
        _cryptoProvider = cryptoProvider;
    }

    public async ValueTask<JsonSerializerSettings> CreateForSerializationAsync(
        TAggregateId aggregateId,
        Type objectType)
        => new()
        {
            ContractResolver = objectType.HasEncryptedProperties()
                ? new SerializationContractResolver(
                    _cryptoProvider,
                    await GetEncryptionKey(aggregateId),
                    _loggerFactory.CreateLogger<SerializationContractResolver>())
                : new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()}
        };

    private async Task<EncryptionKey?> GetEncryptionKey(TAggregateId aggregateId)
        => _encryptionKeyStore is not null
            ? await _encryptionKeyStore.GetOrCreateKeyAsync(aggregateId)
            : null;

    public async ValueTask<JsonSerializerSettings> CreateForDeserializationAsync(TAggregateId aggregateId)
        => new()
        {
            ContractResolver = new DeserializationContractResolver(
                _cryptoProvider,
                await GetDecryptionKey(aggregateId))
        };

    private async Task<EncryptionKey?> GetDecryptionKey(TAggregateId aggregateId)
        => _encryptionKeyStore is not null
            ? await _encryptionKeyStore.GetKeyAsync(aggregateId)
            : null;
}