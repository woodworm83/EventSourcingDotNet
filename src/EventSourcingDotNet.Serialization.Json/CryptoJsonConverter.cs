using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class CryptoJsonConverter : JsonConverter
{
    private readonly ICryptoTransform _cryptoTransform;

    public CryptoJsonConverter(ICryptoTransform cryptoTransform)
    {
        _cryptoTransform = cryptoTransform;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, _cryptoTransform, CryptoStreamMode.Write))
        {
            using var cryptoWriter = new StreamWriter(cryptoStream);
            serializer.Serialize(cryptoWriter, value);
            cryptoWriter.Flush();
            cryptoStream.FlushFinalBlock();
        }
        serializer.Serialize(writer, memoryStream.ToArray());
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var token = JToken.ReadFrom(reader);
        
        if (!TryGetEncryptedValue(token, out var encryptedValue))
            return token.ToObject(objectType, serializer);

        using var memoryStream = new MemoryStream(encryptedValue);
        using var cryptoStream = new CryptoStream(memoryStream, _cryptoTransform, CryptoStreamMode.Read);
        using var cryptoReader = new StreamReader(cryptoStream);
        return serializer.Deserialize(cryptoReader, objectType);
    }
    
    private static bool TryGetEncryptedValue(JToken token, [MaybeNullWhen(false)] out byte[] encryptedValue)
    {
        encryptedValue = null;
        if (token is not JValue {Type: JTokenType.String, Value: string value} jsonObject) return false;
        
        encryptedValue = Convert.FromBase64String(value);
        return true;
    }

    public override bool CanConvert(Type objectType) => true;
}