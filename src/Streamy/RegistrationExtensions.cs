using Microsoft.Extensions.DependencyInjection;

namespace Streamy;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEventSourcing(this IServiceCollection services)
        => services
            .AddTransient(typeof(IAggregateRepository<,>), typeof(AggregateRepository<,>));
}