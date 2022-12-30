using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace EventSourcingDotNet.FileStorage.UnitTests;

public class RegistrationTests
{
    private static ServiceProvider BuildServiceProvider()
    {
        return new ServiceCollection()
            .AddSingleton(Mock.Of<ICryptoProvider>())
            .AddSingleton(Options.Create(new EncryptionKeyStoreSettings("")))
            .AddFileEncryptionKeyStore(Mock.Of<IConfigurationSection>())
            .BuildServiceProvider();
    }

    [Fact]
    public void ShouldResolveEncryptionKeyStore()
    {
        var serviceProvider = BuildServiceProvider();
        
        var eventStore = serviceProvider.GetService<IEncryptionKeyStore<TestId>>();

        eventStore.Should().BeOfType<EncryptionKeyStore<TestId>>();
    }
}