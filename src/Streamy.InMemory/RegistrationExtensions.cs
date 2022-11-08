using Microsoft.Extensions.DependencyInjection;

namespace Streamy.InMemory;

public static class RegistrationExtensions
{
    public static IServiceCollection AddInMemoryEventStore<TAggregateId>(
        this IServiceCollection services)
        where TAggregateId : IAggregateId
        => services
            .AddSingleton<InMemoryEventStore<TAggregateId>>()
            .AddSingleton<IEventStore<TAggregateId>>(
                serviceProvider => serviceProvider.GetRequiredService<InMemoryEventStore<TAggregateId>>())
            .AddSingleton<IEventPublisher<TAggregateId>>(
                serviceProvider => serviceProvider.GetRequiredService<InMemoryEventStore<TAggregateId>>());
}