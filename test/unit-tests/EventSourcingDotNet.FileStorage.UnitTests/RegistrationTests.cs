using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace EventSourcingDotNet.FileStorage.UnitTests;

public class RegistrationTests
{
    private static ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .Build();
        
        return new ServiceCollection()
            .AddSingleton(Mock.Of<ICryptoProvider>())
            .AddFileEncryptionKeyStore(configuration.GetSection("EncryptionKeyStore"))
            .BuildServiceProvider();
    }

    [Fact]
    public void ShouldResolveEncryptionKeyStore()
    {
        var serviceProvider = BuildServiceProvider();
        
        var eventStore = serviceProvider.GetService<IEncryptionKeyStore>();

        eventStore.Should().BeOfType<EncryptionKeyStore>();
    }
}