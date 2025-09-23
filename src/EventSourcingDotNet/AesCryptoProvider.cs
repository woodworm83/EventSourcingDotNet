using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text.Json;

namespace EventSourcingDotNet;

public sealed class AesCryptoProvider(ILogger<AesCryptoProvider> logger) : ICryptoProvider
{
    public void Encrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey)
    {
        using var cryptoStream = new CryptoStream(
            outputStream,
            new ToBase64Transform(),
            CryptoStreamMode.Write,
            leaveOpen: true);

        using var jsonWriter = new Utf8JsonWriter(cryptoStream, new() { Indented = false });

        JsonSerializer.Serialize(
            jsonWriter,
            EncryptValue(inputStream, encryptionKey),
            AesCryptoProviderSerializerContext.Default.AesEncryptedValue);
        
        cryptoStream.FlushFinalBlock();
    }

    private static AesEncryptedValue EncryptValue(Stream inputStream, EncryptionKey encryptionKey)
    {
        using var aes = CreateAes();
        aes.GenerateIV();
        aes.Key = encryptionKey.Key;
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        inputStream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();
        return new(aes.IV, memoryStream.ToArray());
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

    private AesEncryptedValue? GetEncryptedValue(Stream inputStream)
    {
        using var cryptoStream = new CryptoStream(
            inputStream,
            new FromBase64Transform(),
            CryptoStreamMode.Read,
            leaveOpen: true);

        try
        {
            return JsonSerializer.Deserialize(
                cryptoStream,
                AesCryptoProviderSerializerContext.Default.NullableAesEncryptedValue);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to read encrypted value");
            return null;
        }
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
}