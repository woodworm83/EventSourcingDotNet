﻿using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Xunit;

namespace EventSourcingDotNet.UnitTests;

public class AesCryptoProviderTests
{
    private static readonly IReadOnlyList<int> LegalAesKeySizes = GetLegalAesKeySizes()
        .Distinct()
        .ToList();

    private static readonly EncryptionKey EncryptionKey
        = new(Convert.FromBase64String("E/fetTS2G/JtwsovU32b4dtx2JD+yjH+v0MItdGi+tI="));
    private static readonly byte[] CypherText 
        = Convert.FromBase64String("eyJpdiI6Im1uUWZ6dkt4aS9QWGZ2d3FQcVNqM3c9PSIsImN5cGhlciI6IklGRjJ5QnlLNHA0b29CRVZxLzM0TkE9PSJ9");
    private const string PlainText = "plainText";

    [Fact]
    public void ShouldGenerateKeyWithValidKeySize()
    {
        var provider = new AesCryptoProvider();
        
        var encryptionKey = provider.GenerateKey();

        LegalAesKeySizes.Should().Contain(encryptionKey.Key.Length * 8);
    }
    
    [Fact]
    public void ShouldDecryptValueWhenEncryptionKeyIsNotNull()
    {
        var provider = new AesCryptoProvider();

        using var inputStream = new MemoryStream(CypherText);
        using var outputStream = new MemoryStream();
        
        provider.TryDecrypt(inputStream, outputStream, EncryptionKey)
            .Should().BeTrue();
        
        Encoding.UTF8.GetString(outputStream.ToArray())
            .Should().Be(PlainText);
    }

    [Fact]
    public void ShouldNotDecryptValueWhenCypherTextDoesNotContainValidData()
    {
        var provider = new AesCryptoProvider();

        using var inputStream = new MemoryStream();
        using var outputStream = new MemoryStream();
        
        provider.TryDecrypt(inputStream, outputStream, EncryptionKey)
            .Should().BeFalse();

        outputStream.Length.Should().Be(0);
    }

    [Fact]
    public void ShouldEncryptValue()
    {
        var provider = new AesCryptoProvider();

        using var inputStream = new MemoryStream(Encoding.UTF8.GetBytes(PlainText));
        using var outputStream = new MemoryStream();
        provider.Encrypt(inputStream, outputStream, EncryptionKey);

        outputStream.Length.Should().BeGreaterThan(0);
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