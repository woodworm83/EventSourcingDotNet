using EventSourcingDotNet.Serialization.Json;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventSourcingDotNet.EventStore;

internal sealed class EventStoreProvider : IEventStoreProvider
{
    private readonly string? _connectionString;

    public EventStoreProvider(string? connectionString = null)
    {
        _connectionString = connectionString;
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

        if (_connectionString is not null)
        {
            var options = Options.Create(EventStoreClientSettings.Create(_connectionString));
            services
                .AddTransient(
                    typeof(IEventStore<>).MakeGenericType(aggregateIdType),
                    serviceProvider => ActivatorUtilities.CreateInstance(
                        serviceProvider,
                        typeof(EventStore<>).MakeGenericType(aggregateIdType),
                        options))
                .AddTransient(
                    typeof(IEventPublisher<>).MakeGenericType(aggregateIdType),
                    serviceProvider => ActivatorUtilities.CreateInstance(
                        serviceProvider,
                        typeof(EventPublisher<>).MakeGenericType(aggregateIdType),
                        options));
        }
        else
        {
            services
                .AddTransient(
                    typeof(IEventStore<>).MakeGenericType(aggregateIdType),
                    typeof(EventStore<>).MakeGenericType(aggregateIdType))
                .AddTransient(
                    typeof(IEventPublisher<>).MakeGenericType(aggregateIdType),
                    typeof(EventPublisher<>).MakeGenericType(aggregateIdType));
        }
    }
}