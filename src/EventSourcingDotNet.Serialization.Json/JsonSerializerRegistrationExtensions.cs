using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.Serialization.Json;

public static class JsonSerializerRegistrationExtensions
{
    public static IServiceCollection AddJsonSerializer(this IServiceCollection services)
        => services
            .AddTransient<IJsonSerializerSettingsFactory, JsonSerializerSettingsFactory>();
}