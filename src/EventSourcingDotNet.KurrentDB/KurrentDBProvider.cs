using EventSourcingDotNet.Serialization.Json;
using KurrentDB.Client;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.KurrentDB;

// ReSharper disable once InconsistentNaming
internal sealed class KurrentDBProvider : IEventStoreProvider
{
    private readonly KurrentDBClientSettings _clientSettings;
    private readonly EventSerializerSettings _eventSerializerSettings;

    public KurrentDBProvider(
        KurrentDBClientSettings clientSettings, 
        EventSerializerSettings eventSerializerSettings)
    {
        _clientSettings = clientSettings;
        _eventSerializerSettings = eventSerializerSettings;
    }

    public void RegisterServices(IServiceCollection services)
    {
        services
            .AddSingleton(new KurrentDBClient(_clientSettings))
            .AddSingleton<IEventReader, EventReader>()
            .AddSingleton<IEventListener, EventListener>()
            .AddSingleton(_eventSerializerSettings)
            .AddTransient<IEventSerializer, EventSerializer>()
            .AddTransient(typeof(IEventStore<>), typeof(EventStore<>))
            .AddJsonSerializer();
    }
}