namespace EventSourcingDotNet;

[AttributeUsage(AttributeTargets.Property)]
public class EncryptedAttribute : Attribute
{
}

public readonly record struct EncryptionKey(byte[] Key, byte[] Nonce);

public interface IEncryptionKeyStore<in TAggregateId>
    where TAggregateId : IAggregateId
{
    ValueTask<EncryptionKey> GetOrCreateKeyAsync(TAggregateId aggregateId);

    ValueTask<EncryptionKey?> GetKeyAsync(TAggregateId aggregateId);

    ValueTask DeleteKeyAsync(TAggregateId aggregateId);
}