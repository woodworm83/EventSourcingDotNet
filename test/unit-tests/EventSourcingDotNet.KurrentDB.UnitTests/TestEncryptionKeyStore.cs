using System.Collections.Concurrent;

namespace EventSourcingDotNet.KurrentDB.UnitTests;

internal sealed class TestEncryptionKeyStore : IEncryptionKeyStore
{
    private readonly ConcurrentDictionary<string, EncryptionKey> _encryptionKeys = new(StringComparer.Ordinal);
    private readonly ICryptoProvider _cryptoProvider;

    public TestEncryptionKeyStore(ICryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider;
    }

    public ValueTask<EncryptionKey> GetOrCreateKeyAsync(string encryptionKeyName)
        => ValueTask.FromResult(
            _encryptionKeys.GetOrAdd(encryptionKeyName, _ => _cryptoProvider.GenerateKey()));

    public ValueTask<EncryptionKey?> GetKeyAsync(string encryptionKeyName)
        => ValueTask.FromResult<EncryptionKey?>(
            _encryptionKeys.TryGetValue(encryptionKeyName, out var encryptionKey)
                ? encryptionKey
                : null);

    public ValueTask DeleteKeyAsync(string encryptionKeyName)
    {
        _encryptionKeys.TryRemove(encryptionKeyName, out _);
        return ValueTask.CompletedTask;
    }
}