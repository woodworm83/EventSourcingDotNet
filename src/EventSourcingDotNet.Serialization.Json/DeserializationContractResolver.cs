using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class DeserializationContractResolver : DefaultContractResolver
{
    private readonly ICryptoTransform? _decryptor;
    private readonly ILoggerFactory _loggerFactory;

    public DeserializationContractResolver(ICryptoTransform? decryptor, ILoggerFactory loggerFactory)
    {
        _decryptor = decryptor;
        _loggerFactory = loggerFactory;

        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        foreach (var jsonProperty in properties)
        {
            jsonProperty.Converter = new CryptoJsonConverter(_decryptor, _loggerFactory.CreateLogger<CryptoJsonConverter>());
        }

        return properties;
    }
}