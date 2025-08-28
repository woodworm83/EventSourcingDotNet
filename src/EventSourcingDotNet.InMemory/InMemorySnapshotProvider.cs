using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshotProvider : ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType, Type stateType)
    {
        var implementationType = typeof(InMemorySnapshotStore<,>).MakeGenericType(aggregateIdType, stateType);
        var serviceType = typeof(ISnapshotStore<,>).MakeGenericType(aggregateIdType, stateType);

        services
            .AddSingleton(implementationType)
            .AddHostedService(ImplementationFactory)
            .AddSingleton(serviceType, ImplementationFactory);
        
        IHostedService ImplementationFactory(IServiceProvider serviceProvider) =>
            (IHostedService)serviceProvider.GetRequiredService(implementationType);
    }
}