using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public interface IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services);
}