using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.InMemory;

public sealed class InMemorySnapshotProvider : ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType, Type stateType)
        => services.AddSingleton(
            typeof(ISnapshotStore<,>).MakeGenericType(aggregateIdType, stateType),
            typeof(InMemorySnapshotStore<,>).MakeGenericType(aggregateIdType, stateType));
}