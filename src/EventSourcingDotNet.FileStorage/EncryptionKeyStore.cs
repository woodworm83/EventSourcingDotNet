using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.FileStorage;

public sealed class EncryptionKeyStoreSettings
{
    public string? StoragePath { get; set; }
};

internal sealed class EncryptionKeyStore : IEncryptionKeyStore
{
    private readonly EncryptionKeyStoreSettings _settings;
    private readonly ICryptoProvider _cryptoProvider;

    public EncryptionKeyStore(IOptions<EncryptionKeyStoreSettings> settings, ICryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider;
        _settings = settings.Value;
    }

    public async ValueTask<EncryptionKey> GetOrCreateKeyAsync(string encryptionKeyName)
    {
        var file = GetFileInfo(encryptionKeyName);
        if (await GetKeyAsync(file) is { } encryptionKey) return encryptionKey;

        if (file.Directory is {Exists: false} directory)
        {
            directory.Create();
        }

        encryptionKey = _cryptoProvider.GenerateKey();
        await File.WriteAllBytesAsync(file.FullName, encryptionKey.Key);
        return encryptionKey;
    }

    public async ValueTask<EncryptionKey?> GetKeyAsync(string encryptionKeyName)
        => await GetKeyAsync(GetFileInfo(encryptionKeyName));
    
    private static async ValueTask<EncryptionKey?> GetKeyAsync(FileInfo file)
    {
        if (!file.Exists) return null;

        return new EncryptionKey(await File.ReadAllBytesAsync(file.FullName));
    }

    public ValueTask DeleteKeyAsync(string encryptionKeyName)
    {
        if (GetFileInfo(encryptionKeyName) is {Exists: true} file)
        {
            file.Delete();
        }

        return ValueTask.CompletedTask;
    }
    
    private FileInfo GetFileInfo(string encryptionKeyName)
    {
        var storagePath = _settings.StoragePath ?? "keys";
        return new FileInfo(
            Path.Combine(storagePath, encryptionKeyName));
    }
}