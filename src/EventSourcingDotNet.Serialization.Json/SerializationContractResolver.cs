using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class SerializationContractResolver : DefaultContractResolver
{
    private readonly ICryptoProvider? _cryptoProvider;
    private readonly EncryptionKey? _encryptionKey;
    private readonly ILogger<SerializationContractResolver> _logger;

    public SerializationContractResolver(ICryptoProvider? cryptoProvider, EncryptionKey? encryptionKey, ILogger<SerializationContractResolver> logger)
    {
        _cryptoProvider = cryptoProvider;
        _encryptionKey = encryptionKey;
        _logger = logger;

        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        if (!type.HasEncryptedProperties()) return properties;
        if (!CheckCryptoProviderAndEncryptionKey(type)) return properties;

        var cryptoJsonConverter = new CryptoJsonConverter(_cryptoProvider, _encryptionKey.Value);
        
        foreach (var jsonProperty in properties)
        {
            if (!jsonProperty.HasEncryptedAttribute(type)) continue;

            jsonProperty.PropertyName = $"#{jsonProperty.PropertyName}";
            jsonProperty.Converter = cryptoJsonConverter;
        }

        return properties;
    }

    [MemberNotNullWhen(true, nameof(_cryptoProvider), nameof(_encryptionKey))]
    private bool CheckCryptoProviderAndEncryptionKey(Type type)
    {
        if (_cryptoProvider is null)
        {
            _logger.LogWarning(
                @"Cannot encrypt properties of type {Type} with EncryptAttribute: No crypto provider available", type);
            return false;
        }

        if (_encryptionKey is null)
        {
            _logger.LogWarning(
                @"Cannot encrypt properties of type {Type} with EncryptAttribute: No encryption key available", type.Name);
            return false;
        }

        return true;
    }
}