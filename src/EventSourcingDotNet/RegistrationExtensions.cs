using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public static class RegistrationExtensions
{
    public delegate void ConfigureEventSourcing(EventSourcingBuilder builder);

    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        ConfigureEventSourcing configure)
    {
        var builder = new EventSourcingBuilder();
        configure(builder);
        builder.ConfigureServices(services);
        return services
            .AddSingleton<IEventTypeResolver, EventTypeResolver>()
            .AddTransient(typeof(IAggregateRepository<,>), typeof(AggregateRepository<,>));
    }

    public static EventSourcingBuilder UseAesCryptoProvider(this EventSourcingBuilder builder)
        => builder.UseCryptoProvider<AesCryptoProvider>();
}