using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
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

        var deserialized = Encoding.UTF8.GetString(
            Convert.FromBase64String(
                JsonConvert.DeserializeObject<string>(encrypted)!));

        deserialized
            .Should().NotBeSameAs(_plainText);
    }

    [Fact]
    public void ShouldDecryptPropertyValue()
    {
        const string encrypted = @"""9yrUdI04WE4RXqHU8wHO0A==""";

        var decrypted = Deserialize<string>(encrypted);

        decrypted.Should().Be(_plainText);
    }

    private string Serialize(object value)
    {
        var encryptor = new CryptoJsonConverter(CreateEncryptor());

        using var writer = new StringWriter();
        using var jsonWriter = new JsonTextWriter(writer);
        
        encryptor.WriteJson(jsonWriter, value, new JsonSerializer());
        
        return writer.ToString();
    }

    private T? Deserialize<T>(string encrypted)
    {
        var decryptor = new CryptoJsonConverter(CreateDecryptor());

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