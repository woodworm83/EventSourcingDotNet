using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.InMemory;

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
                typeof(IEventListener<>).MakeGenericType(aggregateIdType),
                ImplementationFactory);
        
        object ImplementationFactory(IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService(eventStoreType);
    }
}