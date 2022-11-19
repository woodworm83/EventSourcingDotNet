using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcingDotNet;

public static class RegistrationExtensions
{
    public static IServiceCollection AddEventSourcing(
        this IServiceCollection services,
        Action<EventSourcingBuilder> configure)
    {
        var builder = new EventSourcingBuilder();
        configure(builder);
        builder.ConfigureServices(services);
        return services
            .AddTransient(typeof(IAggregateRepository<,>), typeof(AggregateRepository<,>));
    }
}

public sealed class EventSourcingBuilder
{
    private readonly Dictionary<Type, AggregateBuilder> _aggregates = new();

    public AggregateBuilder AddAggregate<TAggregateId>()
    {
        var builder = new AggregateBuilder();
        _aggregates[typeof(TAggregateId)] = builder;
        return builder;
    }

    public AggregateBuilder Scan(params Assembly[] assemblies)
    {
        var builder = new AggregateBuilder();
        var aggregateIdTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IAggregateId)));

        foreach (var type in aggregateIdTypes)
        {
            _aggregates[type] = builder;
        }

        return builder;
    }

    public AggregateBuilder Scan(params Type[] assemblyMarkerTypes)
        => Scan(
            assemblyMarkerTypes
                .Select(type => type.Assembly)
                .Distinct()
                .ToArray());

    internal void ConfigureServices(IServiceCollection serviceCollection)
    {
        foreach (var (aggregateIdType, builder) in _aggregates)
        {
            builder.ConfigureServices(serviceCollection, aggregateIdType);
        }
    }
}

public interface ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType);
}

public interface IEventStoreProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType);
}

public sealed class AggregateBuilder
{
    private IEventStoreProvider? _eventStoreProvider;
    private ISnapshotProvider? _snapshotProvider;

    public AggregateBuilder UseEventStoreProvider(IEventStoreProvider provider)
    {
        _eventStoreProvider = provider;
        return this;
    }

    public AggregateBuilder UseSnapshotProvider(ISnapshotProvider provider)
    {
        _snapshotProvider = provider;
        return this;
    }

    internal void ConfigureServices(IServiceCollection services, Type aggregateIdType)
    {
        if (_eventStoreProvider is not { } eventStoreProvider)
            throw new InvalidOperationException($"Event store provider was not specified");
            
        eventStoreProvider.RegisterServices(services, aggregateIdType);

        if (_snapshotProvider is { } snapshotProvider)
        {
            snapshotProvider.RegisterServices(services, aggregateIdType);
        }
    }
}