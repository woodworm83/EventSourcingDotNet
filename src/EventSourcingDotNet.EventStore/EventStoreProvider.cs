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

    public void RegisterServices(IServiceCollection services, Type aggregateIdType)
    {
        services
            .AddTransient(
                typeof(IEventSerializer<>).MakeGenericType(aggregateIdType),
                typeof(EventSerializer<>).MakeGenericType(aggregateIdType))
            .AddSingleton(
                typeof(IEventTypeResolver<>).MakeGenericType(aggregateIdType),
                typeof(EventTypeResolver<>).MakeGenericType(aggregateIdType))
            .AddJsonSerializer(aggregateIdType);

        if (_clientSettings is not null)
        {
            services
                .AddTransient(
                    typeof(IEventStore<>).MakeGenericType(aggregateIdType),
                    serviceProvider => ActivatorUtilities.CreateInstance(
                        serviceProvider,
                        typeof(EventStore<>).MakeGenericType(aggregateIdType),
                        Options.Create(_clientSettings)))
                .AddTransient(
                    typeof(IEventListener<>).MakeGenericType(aggregateIdType),
                    serviceProvider => ActivatorUtilities.CreateInstance(
                        serviceProvider,
                        typeof(EventListener<>).MakeGenericType(aggregateIdType),
                        Options.Create(_clientSettings)));
        }
        else
        {
            services
                .AddTransient(
                    typeof(IEventStore<>).MakeGenericType(aggregateIdType),
                    typeof(EventStore<>).MakeGenericType(aggregateIdType))
                .AddTransient(
                    typeof(IEventListener<>).MakeGenericType(aggregateIdType),
                    typeof(EventListener<>).MakeGenericType(aggregateIdType));
        }
    }
}