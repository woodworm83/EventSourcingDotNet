using JetBrains.Annotations;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.KurrentDB;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class RegistrationExtensions
{
    public static IServiceCollection ConfigureKurrentDB(this IServiceCollection services, string connectionString)
        => services
            .AddSingleton(Options.Create(KurrentDBClientSettings.Create(connectionString)));
    
    public static void AddKurrentDBServices(
        this IServiceCollection services,
        KurrentDBClientSettings clientSettings)
        => services
            .AddSingleton(new KurrentDBClient(clientSettings))
            .AddSingleton<IEventReader, EventReader>()
            .AddSingleton<IEventListener, EventListener>()
            .AddTransient(typeof(IEventStore<>), typeof(EventStore<>));
}