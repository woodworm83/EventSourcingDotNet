using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStoreProvider : IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType)
    {
        var eventStoreType = typeof(InMemoryEventStore<>).MakeGenericType(aggregateIdType);
        
        services.TryAddSingleton<IInMemoryEventStream, InMemoryEventStream>();

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