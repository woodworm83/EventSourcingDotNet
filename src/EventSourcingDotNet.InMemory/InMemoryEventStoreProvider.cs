using System.Reactive.Concurrency;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.InMemory;

internal sealed class InMemoryEventStoreProvider(IScheduler? scheduler = null) : IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services)
        => services
            .AddSingleton<IInMemoryEventStream>(_ => new InMemoryEventStream(scheduler))
            .AddSingleton(typeof(IEventStore<>), typeof(InMemoryEventStore<>))
            .AddSingleton<IEventListener, EventListener>()
            .AddSingleton<IEventReader, EventReader>();
}