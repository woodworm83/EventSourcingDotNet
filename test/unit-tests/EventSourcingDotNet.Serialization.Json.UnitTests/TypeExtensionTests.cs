using FluentAssertions;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class TypeExtensionTests
{
    private readonly JsonProperty _property = new() {UnderlyingName = "Property"};

    [Theory]
    [InlineData(typeof(TestTypeWithoutEncryptedProperties), false)]
    [InlineData(typeof(TestTypeWithEncryptedProperties), true)]
    public void ShouldCheckTypeForEncryptedProperties(Type type, bool expectedResult)
    {
        type.HasEncryptedProperties()
            .Should().Be(expectedResult);
    }

    [Theory]
    [InlineData(typeof(TestTypeWithoutEncryptedProperties), false)]
    [InlineData(typeof(TestTypeWithEncryptedProperties), true)]
    public void ShouldCheckPropertyForEncryptedAttribute(Type type, bool expectedResult)
    {
        _property.HasEncryptedAttribute(type)
            .Should().Be(expectedResult);
    }
}