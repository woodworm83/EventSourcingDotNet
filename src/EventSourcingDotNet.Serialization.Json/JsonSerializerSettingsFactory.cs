using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

public sealed class JsonSerializerSettingsFactory : IJsonSerializerSettingsFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ICryptoProvider? _cryptoProvider;
    private readonly IEncryptionKeyStore? _encryptionKeyStore;

    public JsonSerializerSettingsFactory(
        ILoggerFactory loggerFactory, 
        ICryptoProvider? cryptoProvider = null, 
        IEncryptionKeyStore? encryptionKeyStore = null)
    {
        _loggerFactory = loggerFactory;
        _encryptionKeyStore = encryptionKeyStore;
        _cryptoProvider = cryptoProvider;
    }

    public async ValueTask<JsonSerializerSettings> CreateForSerializationAsync(Type objectType, string? encryptionKeyName = null)
        => new()
        {
            ContractResolver = objectType.HasEncryptedProperties()
                ? new SerializationContractResolver(
                    _cryptoProvider,
                    await GetKeyForEncryptionAsync(encryptionKeyName),
                    _loggerFactory.CreateLogger<SerializationContractResolver>())
                : new DefaultContractResolver {NamingStrategy = new CamelCaseNamingStrategy()}
        };

    private async ValueTask<EncryptionKey?> GetKeyForEncryptionAsync(string? encryptionKeyName)
        => _encryptionKeyStore is not null && encryptionKeyName is not null
            ? await _encryptionKeyStore.GetOrCreateKeyAsync(encryptionKeyName)
            : null;

    public async ValueTask<JsonSerializerSettings> CreateForDeserializationAsync(string? encryptionKeyName = null)
        => new()
        {
            ContractResolver = new DeserializationContractResolver(
                _cryptoProvider,
                await GetKeyForDecryptionAsync(encryptionKeyName))
        };

    private async ValueTask<EncryptionKey?> GetKeyForDecryptionAsync(string? encryptionKeyName)
        => _encryptionKeyStore is not null && encryptionKeyName is not null
            ? await _encryptionKeyStore.GetKeyAsync(encryptionKeyName)
            : null;
}