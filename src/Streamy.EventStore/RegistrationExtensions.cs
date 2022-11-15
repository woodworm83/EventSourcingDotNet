using Microsoft.Extensions.DependencyInjection;

namespace Streamy.EventStore;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEventStore<TAggregateId>(IServiceCollection services)
        where TAggregateId : IAggregateId
        => services
            .AddTransient<IEventStore<TAggregateId>, EventStore<TAggregateId>>()
            .AddTransient<IEventSerializer<TAggregateId>, EventSerializer<TAggregateId>>()
            .AddSingleton<IEventTypeResolver<TAggregateId>, EventTypeResolver<TAggregateId>>()
            .AddTransient<IStreamNamingConvention<TAggregateId>, StreamNamingConvention<TAggregateId>>();
}