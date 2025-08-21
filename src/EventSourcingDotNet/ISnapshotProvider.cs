using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public interface ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType, Type stateType);

    public sealed void RegisterServices(IServiceCollection services, Type aggregateIdType, IEnumerable<Type> stateTypes)
    {
        foreach (var stateType in stateTypes)
        {
            RegisterServices(services, aggregateIdType, stateType);
        }
    }
}