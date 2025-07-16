using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class CryptoJsonConverterTests
{
    private const string PlainText = "plainText";

    [Fact]
    public void ShouldEncryptPropertyValue()
    {
        var cryptoProviderMock = new Mock<ICryptoProvider>();
        var writerMock = new Mock<TextWriter>();

        var converter = new CryptoJsonConverter(cryptoProviderMock.Object, new EncryptionKey());
        converter.WriteJson(new JsonTextWriter(writerMock.Object), PlainText, new JsonSerializer());

        cryptoProviderMock.Verify(x => x.Encrypt(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<EncryptionKey>()));
    }

    [Fact]
    public void ShouldDecryptPropertyValue()
    {
        var cryptoProviderMock = new Mock<ICryptoProvider>();
        cryptoProviderMock.Setup(x => x.TryDecrypt(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<EncryptionKey>()))
            .Callback<Stream, Stream, EncryptionKey>(
                (inputStream, outputStream, _) => inputStream.CopyTo(outputStream))
            .Returns(true);
        
        using var memoryStream = new MemoryStream(@""""""u8.ToArray());
        using var reader = new StreamReader(memoryStream);

        var converter = new CryptoJsonConverter(cryptoProviderMock.Object, new EncryptionKey());
        converter.ReadJson(new JsonTextReader(reader), typeof(string), null, new JsonSerializer());

        cryptoProviderMock.Verify(x => x.TryDecrypt(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<EncryptionKey>()));
    }

    [Fact]
    public void ShouldEncryptAndDecryptToOriginalValue()
    {
        var cryptoProvider = new AesCryptoProvider();
        var key = cryptoProvider.GenerateKey();
        var converter = new CryptoJsonConverter(cryptoProvider, key);
        var plain = "secret";

        var encrypted = Encrypt(plain, converter);
        var decrypted = Decrypt(encrypted, converter);

        decrypted.Should().Be(plain);
    }

    private static byte[] Encrypt(string plain, CryptoJsonConverter converter)
    {
        using var stream = new MemoryStream();
        
        using (var writer = new JsonTextWriter(new StreamWriter(stream, leaveOpen: true)))
        {
            converter.WriteJson(writer, plain, new JsonSerializer());
        }

        return stream.ToArray();
    }

    private static string? Decrypt(byte[] encrypted, CryptoJsonConverter converter)
    {
        using var stream = new MemoryStream(encrypted);
        using var reader = new JsonTextReader(new StreamReader(stream));
        return (string?)converter.ReadJson(reader, typeof(string), null, new JsonSerializer());
    }
}