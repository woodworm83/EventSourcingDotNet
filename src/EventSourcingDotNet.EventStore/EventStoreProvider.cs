using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventStoreProvider : IEventStoreProvider
{
    private readonly EventStoreClientSettings? _clientSettings;

    public EventStoreProvider(EventStoreClientSettings? clientSettings = null)
    {
        _clientSettings = clientSettings;
    }

    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddSingleton<IEventReader, EventReader>()
            .AddSingleton<IEventListener, EventListener>()
            .AddTransient(typeof(IEventSerializer<>), typeof(EventSerializer<>))
            .AddSingleton(typeof(IEventTypeResolver<>), typeof(EventTypeResolver<>))
            .AddTransient(typeof(IEventStore<>), typeof(EventStore<>))
            .AddJsonSerializer();

        if (_clientSettings is not null)
        {
            services.AddSingleton(Options.Create(_clientSettings));
        }
    }
}