using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class JsonSerializerSettingsFactoryTests
{
    [Fact]
    public async Task ShouldReturnDefaultContractResolverWhenTypeDoesNotHaveEncryptedProperties()
    {
        var settingsFactory = new JsonSerializerSettingsFactory<TestId>(NullLoggerFactory.Instance);

        var settings = await settingsFactory.CreateForSerializationAsync(new TestId(), typeof(TestTypeWithoutEncryptedProperties));

        settings.ContractResolver.Should().BeOfType<DefaultContractResolver>();
    }

    [Fact]
    public async Task ShouldReturnSerializationContractResolverWhenTypeHasEncryptedProperties()
    {
        var settingsFactory = new JsonSerializerSettingsFactory<TestId>(NullLoggerFactory.Instance);

        var settings = await settingsFactory.CreateForSerializationAsync(new TestId(), typeof(TestTypeWithEncryptedProperties));

        settings.ContractResolver.Should().BeOfType<SerializationContractResolver>();
    }

    [Fact]
    public async Task ShouldReturnDeserializationContractResolver()
    {
        var settingsFactory = new JsonSerializerSettingsFactory<TestId>(NullLoggerFactory.Instance);

        var settings = await settingsFactory.CreateForDeserializationAsync(new TestId());

        settings.ContractResolver.Should().BeOfType<DeserializationContractResolver>();
    }
}