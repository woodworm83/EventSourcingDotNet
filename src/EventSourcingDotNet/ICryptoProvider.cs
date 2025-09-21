namespace EventSourcingDotNet;

public interface ICryptoProvider
{
    public void Encrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey);

    public bool TryDecrypt(Stream inputStream, Stream outputStream, EncryptionKey encryptionKey);

    public EncryptionKey GenerateKey();
}