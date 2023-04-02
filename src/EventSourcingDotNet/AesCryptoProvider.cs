using System.Security.Cryptography;
using Newtonsoft.Json;

namespace EventSourcingDotNet;

public sealed class AesCryptoProvider : ICryptoProvider
{
    public void Encrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey)
    {
        var converter = new JsonSerializer();
        using var jsonWriter = new StreamWriter(outputStream, leaveOpen: true);
        converter.Serialize(jsonWriter, EncryptValue(inputStream, encryptionKey));
    }

    private static EncryptedValue EncryptValue(Stream inputStream, EncryptionKey encryptionKey)
    {
        using var aes = CreateAes();
        aes.GenerateIV();
        aes.Key = encryptionKey.Key;
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        inputStream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();
        return new EncryptedValue(aes.IV, memoryStream.ToArray());
    }

    public bool TryDecrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey)
    {
        if (GetEncryptedValue(inputStream) is not { } encryptedValue) return false;

        using var aes = Aes.Create();
        using var cryptoStream = new CryptoStream(
            new MemoryStream(encryptedValue.CypherText),
            aes.CreateDecryptor(encryptionKey.Key, encryptedValue.InitializationVector),
            CryptoStreamMode.Read);
        cryptoStream.CopyTo(outputStream);

        return true;
    }

    private static EncryptedValue? GetEncryptedValue(Stream inputStream)
    {
        var serializer = new JsonSerializer();
        using var jsonReader = new JsonTextReader(new StreamReader(inputStream));

        return serializer.Deserialize<EncryptedValue?>(jsonReader);
    }

    public EncryptionKey GenerateKey()
    {
        using var aes = CreateAes();
        aes.GenerateKey();
        return new EncryptionKey(aes.Key);
    }

    private static Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    private readonly record struct EncryptedValue(
        [property: JsonProperty("iv")] byte[] InitializationVector,
        [property: JsonProperty("cypher")] byte[] CypherText);
}