using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshotProvider : ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType, Type stateType)
    {
        var snapshotType = typeof(InMemorySnapshotStore<,>).MakeGenericType(aggregateIdType, stateType);
        var serviceType = typeof(ISnapshotStore<,>).MakeGenericType(aggregateIdType, stateType);

        services
            .AddSingleton(snapshotType)
            .AddSingleton(typeof(IHostedService), ImplementationFactory)
            .AddSingleton(serviceType, ImplementationFactory);
        
        object ImplementationFactory(IServiceProvider serviceProvider) =>
            serviceProvider.GetRequiredService(snapshotType);
    }
}