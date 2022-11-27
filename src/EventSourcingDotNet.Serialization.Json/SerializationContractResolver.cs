using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class SerializationContractResolver : DefaultContractResolver
{
    private readonly ICryptoTransform? _encryptor;
    private readonly ILoggerFactory _loggerFactory;

    public SerializationContractResolver(ICryptoTransform? encryptor, ILoggerFactory loggerFactory)
    {
        _encryptor = encryptor;
        _loggerFactory = loggerFactory;

        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        foreach (var jsonProperty in properties)
        {
            if (!jsonProperty.HasEncryptedAttribute(type)) continue;

            jsonProperty.Converter = new CryptoJsonConverter(_encryptor, _loggerFactory.CreateLogger<CryptoJsonConverter>());
        }

        return properties;
    }
}