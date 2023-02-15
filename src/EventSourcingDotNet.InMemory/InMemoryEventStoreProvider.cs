using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStoreProvider : IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services)
        => services
            .AddSingleton<IInMemoryEventStream, InMemoryEventStream>()
            .AddSingleton(typeof(IEventStore<>), typeof(InMemoryEventStore<>))
            .AddSingleton<IEventListener, EventListener>()
            .AddSingleton<IEventReader, EventReader>();
}