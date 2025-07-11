using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventStoreProvider : IEventStoreProvider
{
    private readonly EventStoreClientSettings _clientSettings;
    private readonly EventSerializerSettings _eventSerializerSettings;

    public EventStoreProvider(
        EventStoreClientSettings clientSettings, 
        EventSerializerSettings eventSerializerSettings)
    {
        _clientSettings = clientSettings;
        _eventSerializerSettings = eventSerializerSettings;
    }

    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddSingleton(new EventStoreClient(_clientSettings))
            .AddSingleton<IEventReader, EventReader>()
            .AddSingleton<IEventListener, EventListener>()
            .AddSingleton(_eventSerializerSettings)
            .AddTransient<IEventSerializer, EventSerializer>()
            .AddTransient(typeof(IEventStore<>), typeof(EventStore<>))
            .AddJsonSerializer();
    }
}