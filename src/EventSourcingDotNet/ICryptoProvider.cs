namespace EventSourcingDotNet;

public interface ICryptoProvider
{
    void Encrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey);

    bool TryDecrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey);
    
    EncryptionKey GenerateKey();
}

// ReSharper disable once ClassNeverInstantiated.Global