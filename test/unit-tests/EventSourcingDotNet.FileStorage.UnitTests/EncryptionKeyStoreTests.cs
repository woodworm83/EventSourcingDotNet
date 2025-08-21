using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventSourcingDotNet.FileStorage.UnitTests;

public sealed class EncryptionKeyStoreTests : IDisposable
{
    private readonly string _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly byte[] _key = GenerateKey(42);

    private IOptions<EncryptionKeyStoreSettings> Settings
        => Options.Create(new EncryptionKeyStoreSettings { StoragePath = _tempPath });
    
    [Fact]
    public async Task ShouldStoreNewEncryptionKeyIfNotExists()
    {
        var cryptoProviderMock = new Mock<ICryptoProvider>();
        cryptoProviderMock.Setup(x => x.GenerateKey()).Returns(new EncryptionKey(_key));
        var keyStore = new EncryptionKeyStore(Settings, cryptoProviderMock.Object);

        await keyStore.GetOrCreateKeyAsync(GetTestKeyName());

        var result = await File.ReadAllBytesAsync(GetTestKeyPath());
        result.Should().Equal(_key);
    }

    [Fact]
    public async Task ShouldReturnExistingKeyIfExists()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath, TestId.AggregateName));
        await File.WriteAllBytesAsync(GetTestKeyPath(), _key);
        var keyStore = new EncryptionKeyStore(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetOrCreateKeyAsync(GetTestKeyName());

        result.Key.Should().Equal(_key);
    }

    [Fact]
    public async Task ShouldReadExistingKey()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath, TestId.AggregateName));
        await File.WriteAllBytesAsync(GetTestKeyPath(), _key);
        var keyStore = new EncryptionKeyStore(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetKeyAsync(GetTestKeyName());

        result.Should().BeOfType<EncryptionKey>()
            .Which
            .Key.Should().Equal(_key);
    }

    [Fact]
    public async Task ShouldReadExistingKeyIfAsStringReturnsNull()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath));
        await File.WriteAllBytesAsync(GetTestKeyPath(), _key);
        var keyStore = new EncryptionKeyStore(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetKeyAsync(GetTestKeyName());

        result.Should().BeOfType<EncryptionKey>()
            .Which
            .Key.Should().Equal(_key);
    }
    
    [Fact]
    public async Task ShouldReturnNullIfKeyDoesNotExist()
    {
        var keyStore = new EncryptionKeyStore(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetKeyAsync(GetTestKeyName());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldDeleteKey()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath, TestId.AggregateName));
        var filePath = GetTestKeyPath();
        await File.WriteAllBytesAsync(filePath, _key);
        var keyStore = new EncryptionKeyStore(Settings, Mock.Of<ICryptoProvider>());

        await keyStore.DeleteKeyAsync(GetTestKeyName());

        File.Exists(filePath).Should().BeFalse();
    }

    private static string GetTestKeyName([CallerMemberName] string testMethodName = "")
        => testMethodName;

    private string GetTestKeyPath([CallerMemberName] string testMethodName = "") 
        => Path.Combine(_tempPath, GetTestKeyName(testMethodName));

    private static byte[] GenerateKey(int seed)
    {
        var random = new Random(seed);
        var buffer = new byte[128];
        random.NextBytes(buffer);
        return buffer;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [ExcludeFromCodeCoverage]
    private readonly record struct TestId(int? Id) : IAggregateId
    {
        public static string AggregateName => "test";

        public string? AsString() => Id?.ToString(CultureInfo.InvariantCulture);
    }
}