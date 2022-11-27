using System.Security.Cryptography;

namespace EventSourcingDotNet;

public interface ICryptoProvider
{
    ICryptoTransform GetEncryptor(EncryptionKey encryptionKey);

    ICryptoTransform? GetDecryptor(EncryptionKey? encryptionKey);

    EncryptionKey GenerateKey();
}

// ReSharper disable once ClassNeverInstantiated.Global