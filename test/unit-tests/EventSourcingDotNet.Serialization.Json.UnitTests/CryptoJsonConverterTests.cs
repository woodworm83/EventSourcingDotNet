using System.Text;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class CryptoJsonConverterTests
{
    private const string _plainText = "plainText";

    [Fact]
    public void ShouldEncryptPropertyValue()
    {
        var cryptoProviderMock = new Mock<ICryptoProvider>();
        var writerMock = new Mock<TextWriter>();

        var converter = new CryptoJsonConverter(cryptoProviderMock.Object, new EncryptionKey());
        converter.WriteJson(new JsonTextWriter(writerMock.Object), _plainText, new JsonSerializer());

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
        
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(@""""""));
        using var reader = new StreamReader(memoryStream);

        var converter = new CryptoJsonConverter(cryptoProviderMock.Object, new EncryptionKey());
        converter.ReadJson(new JsonTextReader(reader), typeof(string), null, new JsonSerializer());

        cryptoProviderMock.Verify(x => x.TryDecrypt(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<EncryptionKey>()));
    }
}