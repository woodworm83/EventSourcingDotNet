using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class DeserializationContractResolver : DefaultContractResolver
{
    private readonly ICryptoProvider? _cryptoProvider;
    private readonly EncryptionKey? _encryptionKey;

    public DeserializationContractResolver(
        ICryptoProvider? cryptoProvider, 
        EncryptionKey? encryptionKey)
    {
        _cryptoProvider = cryptoProvider;
        _encryptionKey = encryptionKey;

        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);
        if (_cryptoProvider is null) return properties;
        if (_encryptionKey is not { } encryptionKey) return properties;

        foreach (var jsonProperty in base.CreateProperties(type, memberSerialization))
        {
            jsonProperty.PropertyName = $"#{jsonProperty.PropertyName}";
            jsonProperty.Converter = new CryptoJsonConverter(_cryptoProvider, encryptionKey);
            properties.Add(jsonProperty);
        }

        return properties;
    }
}