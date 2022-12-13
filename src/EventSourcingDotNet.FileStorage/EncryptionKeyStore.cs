using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.FileStorage;

public sealed record EncryptionKeyStoreSettings(string? StoragePath);

internal sealed class EncryptionKeyStore<TAggregateId> : IEncryptionKeyStore<TAggregateId>
    where TAggregateId : IAggregateId
{
    private readonly EncryptionKeyStoreSettings _settings;
    private readonly ICryptoProvider _cryptoProvider;

    public EncryptionKeyStore(IOptions<EncryptionKeyStoreSettings> settings, ICryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider;
        _settings = settings.Value;
    }

    public async ValueTask<EncryptionKey> GetOrCreateKeyAsync(TAggregateId aggregateId)
    {
        var file = GetFileInfo(aggregateId);
        if (await GetKeyAsync(file) is { } encryptionKey) return encryptionKey;

        if (file.Directory is {Exists: false} directory)
        {
            directory.Create();
        }

        encryptionKey = _cryptoProvider.GenerateKey();
        await File.WriteAllBytesAsync(file.FullName, encryptionKey.Key);
        return encryptionKey;
    }

    public async ValueTask<EncryptionKey?> GetKeyAsync(TAggregateId aggregateId)
        => await GetKeyAsync(GetFileInfo(aggregateId));
    
    private static async ValueTask<EncryptionKey?> GetKeyAsync(FileInfo file)
    {
        if (!file.Exists) return null;

        return new EncryptionKey(await File.ReadAllBytesAsync(file.FullName));
    }

    public ValueTask DeleteKeyAsync(TAggregateId aggregateId)
    {
        if (GetFileInfo(aggregateId) is {Exists: true} file)
        {
            file.Delete();
        }

        return ValueTask.CompletedTask;
    }
    
    private FileInfo GetFileInfo(TAggregateId aggregateId)
    {
        var storagePath = _settings.StoragePath ?? "keys";
        return new FileInfo(
            Path.Combine(storagePath, TAggregateId.AggregateName, aggregateId.AsString() ?? string.Empty));
    }
}