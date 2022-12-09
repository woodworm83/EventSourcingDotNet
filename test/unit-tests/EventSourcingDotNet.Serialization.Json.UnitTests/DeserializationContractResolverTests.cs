using FluentAssertions;
using Moq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class DeserializationContractResolverTests
{
    [Fact]
    public void ShouldUseCryptoJsonConverterForPropertiesPrefixedWithHash()
    {
        var contractResolver = new DeserializationContractResolver(Mock.Of<ICryptoProvider>(), new EncryptionKey());

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["#property"].Converter.Should().BeOfType<CryptoJsonConverter>();
    }
    
    [Fact]
    public void ShouldNotUseCryptoJsonConverterForPropertiesNotPrefixedWithHash()
    {
        var contractResolver = new DeserializationContractResolver(Mock.Of<ICryptoProvider>(), new EncryptionKey());

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["property"].Converter.Should().BeNull();
    }
    
    [Fact]
    public void ShouldNotAddPropertyPrefixedWithHashJsonConverterWhenCryptoProviderIsNull()
    {
        var contractResolver = new DeserializationContractResolver(null, new EncryptionKey());

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties.Should().NotContain(property => property.PropertyName!.StartsWith("#"));
    }
    
    [Fact]
    public void ShouldNotAddPropertyPrefixedWithHashJsonConverterWhenEncryptionKeyIsNull()
    {
        var contractResolver = new DeserializationContractResolver(Mock.Of<ICryptoProvider>(), null);

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties.Should().NotContain(property => property.PropertyName!.StartsWith("#"));
    }
}