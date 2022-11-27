using System.Security.Cryptography;
using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class AesCryptoProviderTests
{
    private static readonly IReadOnlyList<int> _legalAesKeySizes = GetLegalAesKeySizes()
        .Distinct()
        .ToList();
    
    [Fact]
    public void ShouldGenerateKeyWithValidKeySize()
    {
        var provider = new AesCryptoProvider();
        
        var encryptionKey = provider.GenerateKey();

        _legalAesKeySizes.Should().Contain(encryptionKey.Key.Length * 8); 
        encryptionKey.Nonce.Should().BeOfType<byte[]>()
            .Which
            .Length.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void ShouldCreateDecryptorWhenEncryptionKeyIsNotNull()
    {
        var provider = new AesCryptoProvider();

        provider.GetDecryptor(provider.GenerateKey())
            .Should().NotBeNull();
    }

    [Fact]
    public void ShouldReturnNullWhenEncryptionKeyIsNull()
    {
        var provider = new AesCryptoProvider();

        provider.GetDecryptor(null)
            .Should().BeNull();
    }

    [Fact]
    public void ShouldReturnEncryptor()
    {
        var provider = new AesCryptoProvider();

        provider.GetEncryptor(provider.GenerateKey())
            .Should().NotBeNull();
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