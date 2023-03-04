using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventStoreProvider : IEventStoreProvider
{
    private readonly EventStoreClientSettings _clientSettings;

    public EventStoreProvider(EventStoreClientSettings clientSettings)
    {
        _clientSettings = clientSettings;
    }

    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddSingleton(new EventStoreClient(_clientSettings))
            .AddSingleton<IEventReader, EventReader>()
            .AddSingleton<IEventListener, EventListener>()
            .AddTransient<IEventSerializer, EventSerializer>()
            .AddTransient(typeof(IEventStore<>), typeof(EventStore<>))
            .AddJsonSerializer();
    }
}