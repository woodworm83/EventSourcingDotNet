using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class SerializationContractResolverTests
{
    [Fact]
    public void ShouldUseJsonCryptoConverterForPropertiesWithEncryptedAttribute()
    {
        var contractResolver = new SerializationContractResolver(null, NullLoggerFactory.Instance);
        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["property"].Converter.Should().BeOfType<CryptoJsonConverter>();
    }

    [Fact]
    public void ShouldNotUseJsonCryptoConverterForPropertiesWithoutEncryptedAttribute()
    {
        var contractResolver = new SerializationContractResolver(null, NullLoggerFactory.Instance);
        var contract = contractResolver.ResolveContract(typeof(TestTypeWithoutEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["property"].Converter.Should().BeNull();
    }
}