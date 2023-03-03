using EventSourcingDotNet.FileStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

public static class RegistrationExtensions
{
    public static IServiceCollection AddFileEncryptionKeyStore(this IServiceCollection services, IConfigurationSection configSection)
        => services.AddSingleton<IEncryptionKeyStore, EncryptionKeyStore>()
            .Configure<EncryptionKeyStoreSettings>(configSection);
}