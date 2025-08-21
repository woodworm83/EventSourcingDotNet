namespace EventSourcingDotNet;

public interface IEncryptionKeyStore
{
    ValueTask<EncryptionKey> GetOrCreateKeyAsync(string encryptionKeyName);

    ValueTask<EncryptionKey?> GetKeyAsync(string encryptionKeyName);

    ValueTask DeleteKeyAsync(string encryptionKeyName);
}