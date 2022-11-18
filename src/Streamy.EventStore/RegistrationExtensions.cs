using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Streamy.EventStore;

public static class RegistrationExtensions
{
    public static IServiceCollection ConfigureEventStore(this IServiceCollection services, Uri connectionString)
        => services
            .AddSingleton(Options.Create(EventStoreClientSettings.Create(connectionString.ToString())));
    
    public static AggregateBuilder UseEventStore(
        this AggregateBuilder builder)
        => builder.UseEventStoreProvider(new EventStoreProvider());
}