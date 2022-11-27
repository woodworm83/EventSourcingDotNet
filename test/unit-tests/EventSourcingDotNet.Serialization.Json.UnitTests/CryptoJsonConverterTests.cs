using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class CryptoJsonConverterTests
{
    private const string _plainText = "plainText";

    private static readonly EncryptionKey _testKey = new(
        Convert.FromBase64String("NCGQHa2+i43f+FBoHamAFXKgevnQ5QWKqx+K3rit+zQ="),
        Convert.FromBase64String("AeXZFLvLxmveN3eD0OBfYQ=="));
    
    [Fact]
    public void ShouldEncryptPropertyValue()
    {
        var encrypted = Serialize(_plainText);

        JsonConvert.DeserializeObject<EncryptedProperty>(encrypted)
            .Should().BeOfType<EncryptedProperty>()
            .Which
            .Encrypted.Should().NotBeSameAs(Encoding.UTF8.GetBytes(_plainText));
    }

    [Fact]
    public void ShouldNotDecryptPlainPropertyValue()
    {
        var decrypted = Deserialize<string>(@"""plainText""");
        
        decrypted.Should().Be("plainText");
    }

    [Fact]
    public void ShouldReturnDefaultValueWhenEncryptionKeyIsNotPresent()
    {
        var encrypted = Serialize(42);
        
        var decrypted = Deserialize<int>(encrypted, false);

        decrypted.Should().Be(default);
    }

    [Fact]
    public void ShouldDecryptPropertyValue()
    {
        var encrypted = Serialize(_plainText);

        var decrypted = Deserialize<string>(encrypted);

        decrypted.Should().Be(_plainText);
    }

    [Fact]
    public void ShouldLogWarningWhenCryptoProviderIsNotAvailable()
    {
        var loggerMock = new Mock<ILogger<CryptoJsonConverter>>();

        Serialize(_plainText, false, loggerMock.Object);
        
        loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType,Exception,string>>()));
    }

    [Fact]
    public void ShouldAcceptAllTypes()
    {
        var converter = new CryptoJsonConverter(null, new NullLogger<CryptoJsonConverter>());

        converter.CanConvert(typeof(object));
    }

    private string Serialize(object value, bool hasEncryptionKey = true, ILogger<CryptoJsonConverter>? logger = null)
    {
        var encryptor = new CryptoJsonConverter(
            hasEncryptionKey ? CreateEncryptor() : null, 
            logger ??  new NullLogger<CryptoJsonConverter>());

        using var writer = new StringWriter();
        using var jsonWriter = new JsonTextWriter(writer);
        
        encryptor.WriteJson(jsonWriter, value, new JsonSerializer());
        
        return writer.ToString();
    }

    private T? Deserialize<T>(string encrypted, bool hasEncryptionKey = true)
    {
        var decryptor = new CryptoJsonConverter(
            hasEncryptionKey ? CreateDecryptor() : null,
            new NullLogger<CryptoJsonConverter>());
        
        using var reader = new StringReader(encrypted);
        using var jsonReader = new JsonTextReader(reader);

        if (!jsonReader.Read()) throw new InvalidOperationException();
        return decryptor.ReadJson(jsonReader, typeof(T), null, new JsonSerializer()) is T value
            ? value
            : default;
    }

    private ICryptoTransform CreateEncryptor()
    {
        using var aes = Aes.Create();
        return aes.CreateEncryptor(_testKey.Key, _testKey.Nonce);
    }

    private ICryptoTransform CreateDecryptor()
    {
        using var aes = Aes.Create();
        return aes.CreateDecryptor(_testKey.Key, _testKey.Nonce);
    }
}