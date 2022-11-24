using System.Reflection;
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
            .AddTransient(typeof(IAggregateRepository<,>), typeof(AggregateRepository<,>));
    }
}

public sealed class EventSourcingBuilder
{
    private readonly Dictionary<Type, (IReadOnlyList<Type> StateType, AggregateBuilder Builder)> _aggregates = new();

    public AggregateBuilder AddAggregate<TAggregateId, TState>()
        where TAggregateId : IAggregateId
        where TState : IAggregateState<TAggregateId>
    {
        var builder = new AggregateBuilder();
        _aggregates[typeof(TAggregateId)] = (new List<Type>(){ typeof(TState) }, builder);
        return builder;
    }

    public AggregateBuilder AddAggregate<TAggregateId>(params Type[] stateTypes)
        where TAggregateId : IAggregateId
    {
        CheckStateTypes(stateTypes, typeof(TAggregateId));
        var builder = new AggregateBuilder();
        _aggregates[typeof(TAggregateId)] = (stateTypes.ToList(), builder);
        return builder;
    }

    private static void CheckStateTypes(IEnumerable<Type> stateTypes, Type aggregateIdType)
    {
        var expectedStateType = typeof(IAggregateState<>).MakeGenericType(aggregateIdType);
        var invalidTypes = stateTypes
            .Where(t => !t.IsAssignableTo(expectedStateType))
            .ToList();

        switch (invalidTypes)
        {
            case [var stateType]:
                throw GetInvalidStateTypeException(stateType);

            case {Count: > 1}:
                throw new AggregateException(invalidTypes.Select(GetInvalidStateTypeException));
        }

        InvalidOperationException GetInvalidStateTypeException(Type stateType)
        {
            return new InvalidOperationException(
                $"Type {stateType.Name} is not assignable to type {expectedStateType.Name}");
        }
    }
    
    public AggregateBuilder Scan(params Type[] assemblyMarkerTypes)
        => Scan(
            assemblyMarkerTypes
                .Select(type => type.Assembly)
                .Distinct()
                .ToArray());

    private AggregateBuilder Scan(params Assembly[] assemblies)
    {
        var builder = new AggregateBuilder();
        var aggregateIdTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && type.IsAssignableTo(typeof(IAggregateId)))
            .Select(idType => (idType, FindAggregateStates(idType, assemblies)));

        foreach (var (idType, stateTypes) in aggregateIdTypes)
        {
            _aggregates[idType] = (stateTypes, builder);
        }

        return builder;
    }

    private IReadOnlyList<Type> FindAggregateStates(Type aggregateIdType, IEnumerable<Assembly> assemblies)
    {
        var expectedType = typeof(IAggregateState<>).MakeGenericType(aggregateIdType);

        return assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && type.IsAssignableTo(expectedType))
            .ToList();
    }

    internal void ConfigureServices(
        IServiceCollection serviceCollection)
    {
        foreach (var (aggregateIdType, (stateTypes, builder)) in _aggregates)
        {
            builder.ConfigureServices(serviceCollection, aggregateIdType, stateTypes);
        }
    }
}

public interface ISnapshotProvider
{
    public void RegisterServices(IServiceCollection services, Type aggregateIdType, Type stateType);

    public sealed void RegisterServices(IServiceCollection services, Type aggregateIdType, IEnumerable<Type> stateTypes)
    {
        foreach (var stateType in stateTypes)
        {
            RegisterServices(services, aggregateIdType, stateType);
        }
    }
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

    internal void ConfigureServices(IServiceCollection services, Type aggregateIdType, IEnumerable<Type> stateTypes)
    {
        if (_eventStoreProvider is not { } eventStoreProvider)
            throw new InvalidOperationException($"Event store provider was not specified");

        eventStoreProvider.RegisterServices(services, aggregateIdType);

        if (_snapshotProvider is { } snapshotProvider)
        {
            snapshotProvider.RegisterServices(services, aggregateIdType, stateTypes);
        }
    }
}