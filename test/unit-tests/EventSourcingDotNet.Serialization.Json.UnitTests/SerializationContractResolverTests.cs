using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class SerializationContractResolverTests
{
    [Fact]
    public void ShouldUseJsonCryptoConverterForPropertiesWithEncryptedAttribute()
    {
        var contractResolver = new SerializationContractResolver(Mock.Of<ICryptoTransform?>(), new NullLogger<SerializationContractResolver>());
        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["#property"].Converter.Should().BeOfType<CryptoJsonConverter>();
    }

    [Fact]
    public void ShouldNotUseJsonCryptoConverterForPropertiesWithoutEncryptedAttribute()
    {
        var contractResolver = new SerializationContractResolver(null, new NullLogger<SerializationContractResolver>());
        var contract = contractResolver.ResolveContract(typeof(TestTypeWithoutEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["property"].Converter.Should().BeNull();
    }
    
    [Fact]
    public void ShouldLogWarningWhenDecryptorIsNull()
    {
        var loggerMock = new Mock<ILogger<SerializationContractResolver>>();
        var contractResolver = new SerializationContractResolver(null, loggerMock.Object);

        contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));
        
        loggerMock.Verify(x => x.Log(LogLevel.Warning, It.IsAny<Microsoft.Extensions.Logging.EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType,Exception,string>>()));
    }
}