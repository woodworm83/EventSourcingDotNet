using EventSourcingDotNet.EventStore;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

public static class EventStoreRegistrationExtensions
{
    public static IServiceCollection ConfigureEventStore(this IServiceCollection services, string connectionString)
        => services
            .AddSingleton(Options.Create(EventStoreClientSettings.Create(connectionString)));
    
    public static AggregateBuilder UseEventStore(
        this AggregateBuilder builder,
        string? connectionString = null)
        => builder.UseEventStoreProvider(new EventStoreProvider(connectionString));
}