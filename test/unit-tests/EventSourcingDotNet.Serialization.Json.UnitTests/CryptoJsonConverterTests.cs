using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class CryptoJsonConverterTests
{
    private const string _plainText = "plainText";

    private static readonly EncryptionKey _testKey = new(
        Convert.FromBase64String("NCGQHa2+i43f+FBoHamAFXKgevnQ5QWKqx+K3rit+zQ="));

    [Fact]
    public void ShouldEncryptPropertyValue()
    {
        var cryptoProviderMock = new Mock<ICryptoProvider>();
        var writerMock = new Mock<TextWriter>();

        var converter = new CryptoJsonConverter(cryptoProviderMock.Object, new EncryptionKey());
        converter.WriteJson(new JsonTextWriter(writerMock.Object), _plainText, new JsonSerializer());
        
        cryptoProviderMock.Verify(x => x.Encrypt(It.IsAny<Stream>(), It.IsAny<Stream>(), It.IsAny<EncryptionKey>()));
    }

    // [Fact]
    // public void ShouldDecryptPropertyValue()
    // {
    //     const string encrypted = @"""9yrUdI04WE4RXqHU8wHO0A==""";
    //
    //     var decrypted = Deserialize<string>(encrypted);
    //
    //     decrypted.Should().Be(_plainText);
    // }
}