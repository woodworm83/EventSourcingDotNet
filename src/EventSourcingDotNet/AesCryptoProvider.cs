using System.Security.Cryptography;

namespace EventSourcingDotNet;

internal sealed class AesCryptoProvider : ICryptoProvider
{
    public ICryptoTransform GetEncryptor(EncryptionKey encryptionKey)
        => CreateAes().CreateEncryptor(encryptionKey.Key, encryptionKey.Nonce);

    public ICryptoTransform? GetDecryptor(EncryptionKey? encryptionKey)
        => encryptionKey is var (key, nonce) 
            ? CreateAes().CreateDecryptor(key, nonce)
            : null;

    public EncryptionKey GenerateKey()
    {
        using var aes = CreateAes();
        return new EncryptionKey(aes.Key, aes.IV);
    }

    private static Aes CreateAes()
    {
        var aes = Aes.Create();
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }
}