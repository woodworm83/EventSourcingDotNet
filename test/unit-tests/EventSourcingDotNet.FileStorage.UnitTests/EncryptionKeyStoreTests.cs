using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace EventSourcingDotNet.FileStorage.UnitTests;

public sealed class EncryptionKeyStoreTests : IDisposable
{
    private readonly string _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly TestId _aggregateId = new(42);
    private readonly byte[] _key = GenerateKey(42);

    private IOptions<EncryptionKeyStoreSettings> Settings
        => Options.Create(new EncryptionKeyStoreSettings { StoragePath = _tempPath });
    
    [Fact]
    public async Task ShouldStoreNewEncryptionKeyIfNotExists()
    {
        var cryptoProviderMock = new Mock<ICryptoProvider>();
        cryptoProviderMock.Setup(x => x.GenerateKey()).Returns(new EncryptionKey(_key));
        var keyStore = new EncryptionKeyStore<TestId>(Settings, cryptoProviderMock.Object);

        await keyStore.GetOrCreateKeyAsync(_aggregateId);

        var result = await File.ReadAllBytesAsync(GetFilePath(_aggregateId));
        result.Should().Equal(_key);
    }

    [Fact]
    public async Task ShouldReturnExistingKeyIfExists()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath, TestId.AggregateName));
        await File.WriteAllBytesAsync(GetFilePath(_aggregateId), _key);
        var keyStore = new EncryptionKeyStore<TestId>(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetOrCreateKeyAsync(_aggregateId);

        result.Key.Should().Equal(_key);
    }

    [Fact]
    public async Task ShouldReadExistingKey()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath, TestId.AggregateName));
        await File.WriteAllBytesAsync(GetFilePath(_aggregateId), _key);
        var keyStore = new EncryptionKeyStore<TestId>(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetKeyAsync(_aggregateId);

        result.Should().BeOfType<EncryptionKey>()
            .Which
            .Key.Should().Equal(_key);
    }

    [Fact]
    public async Task ShouldReadExistingKeyIfAsStringReturnsNull()
    {
        var aggregateId = new TestId(null);
        Directory.CreateDirectory(Path.Combine(_tempPath));
        await File.WriteAllBytesAsync(GetFilePath(aggregateId), _key);
        var keyStore = new EncryptionKeyStore<TestId>(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetKeyAsync(aggregateId);

        result.Should().BeOfType<EncryptionKey>()
            .Which
            .Key.Should().Equal(_key);
    }
    
    [Fact]
    public async Task ShouldReturnNullIfKeyDoesNotExist()
    {
        var keyStore = new EncryptionKeyStore<TestId>(Settings, Mock.Of<ICryptoProvider>());

        var result = await keyStore.GetKeyAsync(_aggregateId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldDeleteKey()
    {
        Directory.CreateDirectory(Path.Combine(_tempPath, TestId.AggregateName));
        var filePath = GetFilePath(_aggregateId);
        await File.WriteAllBytesAsync(filePath, _key);
        var keyStore = new EncryptionKeyStore<TestId>(Settings, Mock.Of<ICryptoProvider>());

        await keyStore.DeleteKeyAsync(_aggregateId);

        File.Exists(filePath).Should().BeFalse();
    }
    

    private string GetFilePath(TestId aggregateId)
    {
        return Path.Combine(_tempPath, TestId.AggregateName, aggregateId.AsString() ?? string.Empty);
    }

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

        public string? AsString() => Id?.ToString();
    }
}