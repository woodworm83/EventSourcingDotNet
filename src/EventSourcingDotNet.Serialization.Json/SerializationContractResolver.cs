using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class SerializationContractResolver : DefaultContractResolver
{
    private readonly ICryptoTransform? _encryptor;
    private readonly ILogger<SerializationContractResolver> _logger;

    public SerializationContractResolver(ICryptoTransform? encryptor, ILogger<SerializationContractResolver> logger)
    {
        _encryptor = encryptor;
        _logger = logger;

        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        if (!type.HasEncryptedProperties()) return properties;
        if (_encryptor is null)
        {
            _logger.LogWarning(
                @"Cannot encrypt properties of type {Type} with EncryptAttribute: No crypto provider available", type.Name);
            return properties;
        }

        foreach (var jsonProperty in properties)
        {
            if (!jsonProperty.HasEncryptedAttribute(type)) continue;

            jsonProperty.PropertyName = $"#{jsonProperty.PropertyName}";
            jsonProperty.Converter = new CryptoJsonConverter(_encryptor);
        }

        return properties;
    }
}