using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class CryptoJsonConverter : JsonConverter
{
    private readonly ICryptoTransform? _cryptoTransform;
    private readonly ILogger<CryptoJsonConverter> _logger;

    public CryptoJsonConverter(ICryptoTransform? cryptoTransform, ILogger<CryptoJsonConverter> logger)
    {
        _cryptoTransform = cryptoTransform;
        _logger = logger;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (_cryptoTransform is null)
        {
            _logger.LogWarning(
                @"Cannot encrypt property {Property} with EncryptAttribute: No crypto provider available", writer.Path);
            serializer.Serialize(writer, value);
            return;
        }

        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, _cryptoTransform, CryptoStreamMode.Write))
        {
            using var cryptoWriter = new StreamWriter(cryptoStream);
            serializer.Serialize(cryptoWriter, value);
            cryptoWriter.Flush();
            cryptoStream.FlushFinalBlock();
        }

        serializer.Serialize(writer, new EncryptedProperty(memoryStream.ToArray()));
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var token = JToken.ReadFrom(reader);
        
        if (!TryGetEncryptedValue(token, serializer, out var encryptedValue))
            return token.ToObject(objectType, serializer);
        if (_cryptoTransform is null) return GetDefaultValue(objectType);
        
        using var memoryStream = new MemoryStream(encryptedValue);
        using var cryptoStream = new CryptoStream(memoryStream, _cryptoTransform, CryptoStreamMode.Read);
        using var cryptoReader = new StreamReader(cryptoStream);
        return serializer.Deserialize(cryptoReader, objectType);
    }

    private static bool TryGetEncryptedValue(JToken token, JsonSerializer serializer, [MaybeNullWhen(false)] out byte[] encryptedValue)
    {
        encryptedValue = null;
        if (token is not JObject jsonObject) return false;
        if (jsonObject.ToObject<EncryptedProperty?>(serializer) is not { } encryptedProperty) return false;
        
        encryptedValue = encryptedProperty.Encrypted;
        return true;
    }

    private static object? GetDefaultValue(Type type)
        => type.IsValueType
            ? Activator.CreateInstance(type)
            : null;

    public override bool CanConvert(Type objectType) => true;
}