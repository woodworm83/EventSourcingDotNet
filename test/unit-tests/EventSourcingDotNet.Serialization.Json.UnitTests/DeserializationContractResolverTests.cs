using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class DeserializationContractResolverTests
{
    [Fact]
    public void ShouldUseCryptoJsonConverter()
    {
        var contractResolver = new DeserializationContractResolver(null, NullLoggerFactory.Instance);

        var contract = contractResolver.ResolveContract(typeof(TestTypeWithEncryptedProperties));

        contract.Should().BeOfType<JsonObjectContract>()
            .Which
            .Properties["property"].Converter.Should().BeOfType<CryptoJsonConverter>();
    }
}