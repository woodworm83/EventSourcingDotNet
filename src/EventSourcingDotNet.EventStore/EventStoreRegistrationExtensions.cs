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

    public static TBuilder UseEventStore<TBuilder>(this TBuilder builder, string connectionString)
        where TBuilder : IAggregateBuilder<TBuilder>
        => builder.UseEventStore(EventStoreClientSettings.Create(connectionString));

    public static TBuilder UseEventStore<TBuilder>(
        this TBuilder builder,
        EventStoreClientSettings? clientSettings = null)
        where TBuilder : IAggregateBuilder<TBuilder>
        => builder.UseEventStoreProvider(
            new EventStoreProvider(clientSettings));
}