using EventSourcingDotNet.EventStore;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace EventSourcingDotNet;

public static class EventStoreRegistrationExtensions
{
    public static IServiceCollection ConfigureEventStore(this IServiceCollection services, string connectionString)
        => services
            .AddSingleton(Options.Create(EventStoreClientSettings.Create(connectionString)));

    public static EventSourcingBuilder UseEventStore(
        this EventSourcingBuilder builder, 
        string connectionString,
        JsonSerializerSettings? serializerSettings = null)
        => builder.UseEventStore(EventStoreClientSettings.Create(connectionString), serializerSettings);

    public static EventSourcingBuilder UseEventStore(
        this EventSourcingBuilder builder,
        EventStoreClientSettings clientSettings,
        JsonSerializerSettings? serializerSettings = null)
        => builder.UseEventStoreProvider(
            new EventStoreProvider(
                clientSettings, 
                new EventSerializerSettings(serializerSettings)));
}