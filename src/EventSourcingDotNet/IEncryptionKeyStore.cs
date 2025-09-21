namespace EventSourcingDotNet;

public interface IEncryptionKeyStore
{
    public ValueTask<EncryptionKey> GetOrCreateKeyAsync(string encryptionKeyName);

    public ValueTask<EncryptionKey?> GetKeyAsync(string encryptionKeyName);

    public ValueTask DeleteKeyAsync(string encryptionKeyName);
}