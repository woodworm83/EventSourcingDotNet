using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using TestLogging;
using Xunit;
using Xunit.Abstractions;

namespace EventSourcingDotNet.UnitTests;

public sealed class AesCryptoProviderTests(ITestOutputHelper outputHelper)
{
    private static readonly IReadOnlyList<int> _legalAesKeySizes = GetLegalAesKeySizes()
        .Distinct()
        .ToList();

    private static readonly EncryptionKey _encryptionKey
        = new(Convert.FromBase64String("E/fetTS2G/JtwsovU32b4dtx2JD+yjH+v0MItdGi+tI="));

    private static readonly byte[] _cypherText
        = "eyJpdiI6Im1uUWZ6dkt4aS9QWGZ2d3FQcVNqM3c9PSIsImN5cGhlciI6IklGRjJ5QnlLNHA0b29CRVZxLzM0TkE9PSJ9"u8.ToArray();

    private const string _plainText = "plainText";

    private readonly ILoggerFactory _loggerFactory = new LoggerFactory([new TestOutputLoggerProvider(outputHelper)]);

    [Fact]
    public void ShouldGenerateKeyWithValidKeySize()
    {
        var provider = new AesCryptoProvider(_loggerFactory.CreateLogger<AesCryptoProvider>());

        var encryptionKey = provider.GenerateKey();

        _legalAesKeySizes.Should().Contain(encryptionKey.Key.Length * 8);
    }

    [Fact]
    public void ShouldDecryptValueWhenEncryptionKeyIsNotNull()
    {
        var provider = new AesCryptoProvider(_loggerFactory.CreateLogger<AesCryptoProvider>());

        using var inputStream = new MemoryStream(_cypherText);
        using var outputStream = new MemoryStream();

        provider
            .TryDecrypt(inputStream, outputStream, _encryptionKey)
            .Should()
            .BeTrue();

        Encoding
            .UTF8
            .GetString(outputStream.ToArray())
            .Should()
            .Be(_plainText);
    }

    [Fact]
    public void ShouldNotDecryptValueWhenCypherTextDoesNotContainValidData()
    {
        var provider = new AesCryptoProvider(_loggerFactory.CreateLogger<AesCryptoProvider>());

        using var inputStream = new MemoryStream();
        using var outputStream = new MemoryStream();

        provider
            .TryDecrypt(inputStream, outputStream, _encryptionKey)
            .Should()
            .BeFalse();

        outputStream.Length.Should().Be(0);
    }

    [Fact]
    public void ShouldEncryptValue()
    {
        var provider = new AesCryptoProvider(_loggerFactory.CreateLogger<AesCryptoProvider>());

        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(_plainText));
        using var outputStream = new MemoryStream();
        provider.Encrypt(inputStream, outputStream, _encryptionKey);

        outputStream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldEncryptAndDecryptToInitialValue()
    {
        var provider = new AesCryptoProvider(_loggerFactory.CreateLogger<AesCryptoProvider>());

        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(_plainText));
        using var encryptedStream = new MemoryStream();
        provider.Encrypt(inputStream, encryptedStream, _encryptionKey);
        encryptedStream.Seek(0, SeekOrigin.Begin);
        using var decryptedStream = new MemoryStream();

        provider
            .TryDecrypt(encryptedStream, decryptedStream, _encryptionKey)
            .Should()
            .BeTrue();

        Encoding.UTF8.GetString(decryptedStream.ToArray()).Should().Be(_plainText);
    }

    private static IEnumerable<int> GetLegalAesKeySizes()
    {
        using var aes = Aes.Create();

        foreach (var keySizes in aes.LegalKeySizes)
        {
            for (var size = keySizes.MinSize; size <= keySizes.MaxSize; size += keySizes.SkipSize)
            {
                yield return size;
            }
        }
    }
}