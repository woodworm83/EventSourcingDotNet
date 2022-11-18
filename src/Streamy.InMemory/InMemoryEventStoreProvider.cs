using Microsoft.Extensions.DependencyInjection;

namespace Streamy.InMemory;

internal sealed class InMemoryEventStoreProvider : IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType)
    {
        var eventStoreType = typeof(InMemoryEventStore<>).MakeGenericType(aggregateIdType);

        services
            .AddSingleton(eventStoreType)
            .AddSingleton(
                typeof(IEventStore<>).MakeGenericType(aggregateIdType),
                ImplementationFactory)
            .AddSingleton(
                typeof(IEventPublisher<>).MakeGenericType(aggregateIdType),
                ImplementationFactory);
        
        object ImplementationFactory(IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService(eventStoreType);
    }
}