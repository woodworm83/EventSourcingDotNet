using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class DeserializationContractResolverTests
{
    [Fact]
    public void ShouldUseCryptoJsonConverterForPropertiesPrefixedWithHash()
    {
        var contractResolver = new DeserializationContractResolver(Mock.Of<ICryptoTransform>(), NullLoggerFactory.Instance);

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["#property"].Converter.Should().BeOfType<CryptoJsonConverter>();
    }
    
    [Fact]
    public void ShouldNotUseCryptoJsonConverterForPropertiesNotPrefixedWithHash()
    {
        var contractResolver = new DeserializationContractResolver(Mock.Of<ICryptoTransform>(), NullLoggerFactory.Instance);

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["property"].Converter.Should().BeNull();
    }
    
    [Fact]
    public void ShouldNotAddPropertyPrefixedWithHashJsonConverterWhenDecryptorIsNull()
    {
        var contractResolver = new DeserializationContractResolver(null, NullLoggerFactory.Instance);

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties.Should().NotContain(property => property.PropertyName!.StartsWith("#"));
    }
}