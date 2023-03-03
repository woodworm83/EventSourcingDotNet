namespace EventSourcingDotNet;

[AttributeUsage(AttributeTargets.Property)]
public class EncryptedAttribute : Attribute
{
}

public readonly record struct EncryptionKey(byte[] Key);

public interface IEncryptionKeyStore
{
    ValueTask<EncryptionKey> GetOrCreateKeyAsync(string encryptionKeyName);

    ValueTask<EncryptionKey?> GetKeyAsync(string encryptionKeyName);

    ValueTask DeleteKeyAsync(string encryptionKeyName);
}