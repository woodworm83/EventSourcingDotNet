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
        if (_decryptor is null) return properties;

        foreach (var jsonProperty in base.CreateProperties(type, memberSerialization))
        {
            jsonProperty.PropertyName = $"#{jsonProperty.PropertyName}";
            jsonProperty.Converter = new CryptoJsonConverter(_decryptor);
            properties.Add(jsonProperty);
        }

        return properties;
    }
}