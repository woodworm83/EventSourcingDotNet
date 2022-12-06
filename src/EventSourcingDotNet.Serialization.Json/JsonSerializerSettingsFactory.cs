using System.Security.Cryptography;
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

    public async ValueTask<JsonSerializerSettings> CreateForSerializationAsync(TAggregateId aggregateId,
        Type objectType)
        => new()
        {
            ContractResolver = objectType.HasEncryptedProperties()
                ? new SerializationContractResolver(
                    await GetEncryptor(aggregateId),
                    _loggerFactory.CreateLogger<SerializationContractResolver>())
                : new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()}
        };

    private async Task<ICryptoTransform?> GetEncryptor(TAggregateId aggregateId)
        => _encryptionKeyStore is not null
            ? _cryptoProvider?.GetEncryptor(await _encryptionKeyStore.GetOrCreateKeyAsync(aggregateId))
            : null;

    public async ValueTask<JsonSerializerSettings> CreateForDeserializationAsync(TAggregateId aggregateId)
        => new()
        {
            ContractResolver = new DeserializationContractResolver(await GetDecryptor(aggregateId), _loggerFactory)
        };

    private async Task<ICryptoTransform?> GetDecryptor(TAggregateId aggregateId)
        => _encryptionKeyStore is not null
            ? _cryptoProvider?.GetDecryptor(await _encryptionKeyStore.GetKeyAsync(aggregateId))
            : null;
}