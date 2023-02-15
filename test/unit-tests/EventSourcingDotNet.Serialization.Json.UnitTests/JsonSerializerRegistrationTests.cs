using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EventSourcingDotNet.Serialization.Json.UnitTests;

public class JsonSerializerRegistrationTests
{
    [Fact]
    public void ShouldResolveJsonSerializerSettingsFactory()
    {
        var serviceProvider = new ServiceCollection()
            .AddJsonSerializer()
            .AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance)
            .BuildServiceProvider();

        serviceProvider.GetService<IJsonSerializerSettingsFactory<TestId>>()
            .Should().BeOfType<JsonSerializerSettingsFactory<TestId>>();
    }
}