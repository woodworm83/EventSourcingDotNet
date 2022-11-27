using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet.Serialization.Json;

public static class JsonSerializerRegistrationExtensions
{
    public static IServiceCollection AddJsonSerializer(this IServiceCollection services, Type aggregateIdType)
        => services
            .AddTransient(
                typeof(IJsonSerializerSettingsFactory<>).MakeGenericType(aggregateIdType),
                typeof(JsonSerializerSettingsFactory<>).MakeGenericType(aggregateIdType));
}