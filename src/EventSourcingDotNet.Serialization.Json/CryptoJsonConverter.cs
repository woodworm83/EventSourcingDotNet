using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcingDotNet.Serialization.Json;

internal sealed class CryptoJsonConverter : JsonConverter
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly EncryptionKey _encryptionKey;

    public CryptoJsonConverter(ICryptoProvider cryptoProvider, EncryptionKey encryptionKey)
    {
        _cryptoProvider = cryptoProvider;
        _encryptionKey = encryptionKey;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        using var inputStream = Serialize(value, serializer);
        using var outputStream = new MemoryStream();
        _cryptoProvider.Encrypt(inputStream, outputStream, _encryptionKey);
        serializer.Serialize(writer, outputStream.ToArray());
    }

    private static Stream Serialize(object? value, JsonSerializer serializer)
    {
        var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, leaveOpen: true);
        serializer.Serialize(streamWriter, value);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var token = JToken.ReadFrom(reader);
        
        using var inputStream = new MemoryStream();
        if (!TryGetEncryptedValue(token, inputStream))
            return token.ToObject(objectType, serializer);

        using var outputStream = new MemoryStream();
        if (!_cryptoProvider.TryDecrypt(inputStream, outputStream, _encryptionKey)) 
            return null;

        outputStream.Seek(0, SeekOrigin.Begin);

        using var jsonReader = new StreamReader(outputStream);
        return serializer.Deserialize(jsonReader, objectType);
    }
    
    private static bool TryGetEncryptedValue(JToken token, Stream outputStream)
    {
        if (token is not JValue {Type: JTokenType.String, Value: string value}) return false;
        
        outputStream.Write(Convert.FromBase64String(value));
        return true;
    }

    public override bool CanConvert(Type objectType) => true;
}